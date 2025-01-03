using NINA.Core.Model;
using NINA.Plugin.TargetScheduler.Database;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Flats;
using NINA.Plugin.TargetScheduler.Grading;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using System;
using System.Data.Entity.Migrations;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Plugin.TargetScheduler.Sequencer {

    /// <summary>
    /// Handle tasks during the image save pipeline.  If grading isn't enabled, increment the exposure plan
    /// accepted count and write the AcquiredImage record marked accepted.  If grading is enabled, treat the
    /// exposure as pended and queue it for grading.  Also, add the Flat Session ID and Project Name file
    /// patterns.
    ///
    /// An instance of this is created for each plan (which contains a single exposure), so it ends after handling
    /// a single image.
    /// </summary>
    public class ImageSaveWatcher : IImageSaveWatcher {
        private IProfile profile;
        private ProfilePreference profilePreference;
        private IImageSaveMediator imageSaveMediator;
        private ITarget target;
        private IExposure exposure;
        private CancellationToken token;

        public ImageSaveWatcher(IProfile profile, IImageSaveMediator imageSaveMediator, ITarget target, IExposure exposure, CancellationToken token) {
            this.profile = profile;
            this.profilePreference = GetProfilePreference(profile);
            this.imageSaveMediator = imageSaveMediator;
            this.target = target;
            this.exposure = exposure;
            this.token = token;

            imageSaveMediator.ImageSaved += ImageSaved;
            imageSaveMediator.BeforeFinalizeImageSaved += BeforeFinalizeImageSaved;

            TSLogger.Debug($"start watching image saves for {target.Project.Name}/{target.Name}: {exposure.DatabaseId}");
        }

        private Task BeforeFinalizeImageSaved(object sender, BeforeFinalizeImageSavedEventArgs args) {
            string sessionIdentifier = new FlatsExpert().FormatSessionIdentifier(target.Project.SessionId);
            ImagePattern proto = TargetScheduler.FlatSessionIdImagePattern;
            args.AddImagePattern(new ImagePattern(proto.Key, proto.Description) { Value = sessionIdentifier });

            string projectName = target?.Project?.Name ?? string.Empty;
            proto = TargetScheduler.ProjectNameImagePattern;
            args.AddImagePattern(new ImagePattern(proto.Key, proto.Description) { Value = projectName });

            return Task.CompletedTask;
        }

        public void ImageSaved(object sender, ImageSavedEventArgs imageSavedEventArgs) {
            if (imageSavedEventArgs.MetaData.Image.ImageType != "LIGHT") {
                return;
            }

            try {
                bool autoAccepted = !target.Project.EnableGrader;
                int acquiredImageId = UpdateDatabase(imageSavedEventArgs, autoAccepted);
                if (!autoAccepted) {
                    GradingWorkData workData = GetGradingWorkData(acquiredImageId, imageSavedEventArgs);
                    GetImageGradingController().Enqueue(workData, token);
                }
            } catch (Exception ex) {
                TSLogger.Error($"exception in ImageSaveWatcher.ImageSaved: {ex.Message}\n{ex.StackTrace}");
            } finally {
                imageSaveMediator.ImageSaved -= ImageSaved;
                imageSaveMediator.BeforeFinalizeImageSaved -= BeforeFinalizeImageSaved;

                TSLogger.Debug($"done watching image saves for {target.Project.Name}/{target.Name}: {exposure.DatabaseId}");
            }
        }

        public virtual ProfilePreference GetProfilePreference(IProfile profile) {
            return new SchedulerPlanLoader(profile).GetProfilePreferences();
        }

        public virtual SchedulerDatabaseContext GetSchedulerDatabaseContext() {
            return new SchedulerDatabaseInteraction().GetContext();
        }

        public virtual ImageGradingController GetImageGradingController() {
            return ImageGradingController.Instance;
        }

        private GradingWorkData GetGradingWorkData(int acquiredImageId, ImageSavedEventArgs imageSavedEventArgs) {
            ImageGraderPreferences prefs = new ImageGraderPreferences(profile, profilePreference);
            return new GradingWorkData(false, target.DatabaseId, exposure.DatabaseId, acquiredImageId, imageSavedEventArgs, prefs);
        }

        private int UpdateDatabase(ImageSavedEventArgs imageSavedEventArgs, bool accepted) {
            using (var context = GetSchedulerDatabaseContext()) {
                using (var transaction = context.Database.BeginTransaction()) {
                    try {
                        ExposurePlan exposurePlan = context.GetExposurePlan(exposure.DatabaseId);
                        if (exposurePlan != null) {
                            exposurePlan.Acquired++;
                            if (accepted) { exposurePlan.Accepted++; }
                            context.ExposurePlanSet.AddOrUpdate(exposurePlan);
                        } else {
                            TSLogger.Warning($"failed to get exposure plan for id={exposure.DatabaseId}");
                        }

                        AcquiredImage acquiredImage = new AcquiredImage(
                            profile.Id.ToString(),
                            target.Project.DatabaseId,
                            target.DatabaseId,
                            exposure.DatabaseId,
                            imageSavedEventArgs.MetaData.Image.ExposureStart,
                            exposure.FilterName,
                            accepted ? GradingStatus.Accepted : GradingStatus.Pending,
                            string.Empty,
                            new ImageMetadata(imageSavedEventArgs, target.Project.SessionId, target.ROI, exposure.ReadoutMode));
                        AcquiredImage entity = context.AcquiredImageSet.Add(acquiredImage);

                        context.SaveChanges();
                        transaction.Commit();
                        return entity.Id;
                    } catch (Exception e) {
                        TSLogger.Error($"exception updating database for saved image: {e.Message}\n{e.StackTrace}");
                        SchedulerDatabaseContext.CheckValidationErrors(e);
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }

    public interface IImageSaveWatcher { }
}