using NINA.Plugin.TargetScheduler.Database;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Grading;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Plugin.TargetScheduler.Sequencer {

    /// <summary>
    /// Handle tasks during the image save pipeline.  If grading isn't enabled, increment the exposure plan
    /// accepted count and write the AcquiredImage record marked accepted.  If grading is enabled, treat the
    /// exposure as pended and queue it for grading.  Also, add the Flat Session ID and Project Name file
    /// patterns.
    /// </summary>
    public class ImageSaveWatcher : IImageSaveWatcher {
        private object lockObj = new object();
        private IProfile profile;
        private ProfilePreference profilePreference;
        private IImageSaveMediator imageSaveMediator;
        private ConcurrentDictionary<string, ExposureWaitData> exposureDictionary;
        private CancellationToken token;

        public ImageSaveWatcher(IProfile profile, IImageSaveMediator imageSaveMediator, CancellationToken token) {
            this.profile = profile;
            this.profilePreference = GetProfilePreference(profile);
            this.imageSaveMediator = imageSaveMediator;
            this.token = token;
            exposureDictionary = new ConcurrentDictionary<string, ExposureWaitData>(Environment.ProcessorCount * 2, 31);
        }

        /* This isn't going to work if creating one instance per TSContainer.  The reason is that we need a target
         * in BeforeFinalizeImageSaved but this instance could be called for multiple targets ... and we have no
         * way to match up with our data pkg since we don't have the image Id at that point :(
         *
         * There's no way around needing one of these per Target.  So maybe a Dictionary of these in TSContainer:
         *     targetDatabaseId -> ImageSaveWatcher
         * We lookup the correct one when we get a new plan.
         *
         * Think we still need the WaitFor so we can get the exposureId at ImageSave time.
         */

        public void Start() {
            exposureDictionary.Clear();
            imageSaveMediator.ImageSaved += ImageSaved;
            imageSaveMediator.BeforeFinalizeImageSaved += BeforeFinalizeImageSaved;
            TSLogger.Debug($"start watching image saves");
        }

        public void WaitForExposure(string identifier, ExposureWaitData waitData) {
            TSLogger.Debug($"registering waitFor exposure: imageId={waitData.ImageId} exposureId={waitData.ExposureId}");
            exposureDictionary.TryAdd(identifier, waitData);
        }

        public void Stop() {
            TSLogger.Debug($"stopping image save watcher, waiting for exposures to complete:\n{ExposureIdsLog()}");

            // TODO: same TS4 wait for completion HERE

            imageSaveMediator.ImageSaved -= ImageSaved;
            imageSaveMediator.BeforeFinalizeImageSaved -= BeforeFinalizeImageSaved;
            TSLogger.Debug($"stop watching image saves");
        }

        private Task BeforeFinalizeImageSaved(object sender, BeforeFinalizeImageSavedEventArgs args) {
            /*
            string sessionIdentifier = new FlatsExpert().FormatSessionIdentifier(target.Project.SessionId);
            ImagePattern proto = TargetScheduler.FlatSessionIdImagePattern;
            args.AddImagePattern(new ImagePattern(proto.Key, proto.Description) { Value = sessionIdentifier });

            string projectName = target?.Project?.Name ?? string.Empty;
            proto = TargetScheduler.ProjectNameImagePattern;
            args.AddImagePattern(new ImagePattern(proto.Key, proto.Description) { Value = projectName });
            */
            return Task.CompletedTask;
        }

        public async void ImageSaved(object sender, ImageSavedEventArgs imageSavedEventArgs) {
            // NINA code comment says image patterns CAN be injected here ... how?

            /*
            if (imageSavedEventArgs.MetaData.Image.ImageType != "LIGHT") {
                return;
            }

            try {
                TSLogger.Info($"starting ImageSaved: eId={exposure.DatabaseId}, imageId={imageSavedEventArgs.MetaData.Image.Id} ({guid})");
                bool autoAccepted = !target.Project.EnableGrader;
                int acquiredImageId = UpdateDatabase(imageSavedEventArgs, autoAccepted);
                if (!autoAccepted) {
                    GradingWorkData workData = GetGradingWorkData(acquiredImageId, imageSavedEventArgs);
                    await GetImageGradingController().Enqueue(workData, token);
                }
            } catch (Exception ex) {
                TSLogger.Error($"exception in ImageSaveWatcher.ImageSaved: {ex.Message}\n{ex.StackTrace}");
            } finally {
                TSLogger.Info($"done ImageSaved: eId={exposure.DatabaseId}, imageId={imageSavedEventArgs.MetaData.Image.Id} ({guid})");
                imageSaveMediator.ImageSaved -= ImageSaved;
                imageSaveMediator.BeforeFinalizeImageSaved -= BeforeFinalizeImageSaved;

                TSLogger.Debug($"done watching image saves for {target.Project.Name}/{target.Name}: {exposure.DatabaseId}");
            }
            */
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
            return null;
            /*
            ImageGraderPreferences prefs = new ImageGraderPreferences(profile, profilePreference);
            return new GradingWorkData(false, target.DatabaseId, exposure.DatabaseId, acquiredImageId, imageSavedEventArgs, prefs);
            */
        }

        private int UpdateDatabase(ImageSavedEventArgs imageSavedEventArgs, bool accepted) {
            return -1;
            /*
            lock (lockObj) {
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
            */
        }

        private string ExposureIdsLog() {
            StringBuilder sb = new StringBuilder();
            foreach (var entry in exposureDictionary) {
                sb.AppendLine($"{entry.Key}: {entry.Value}");
            }

            return sb.ToString();
        }
    }

    public class ExposureWaitData {
        public int TargetId { get; private set; }
        public int ExposureId { get; private set; }
        public int ImageId { get; private set; }

        public ExposureWaitData(int targetId, int exposureId, int imageId) {
            this.TargetId = targetId;
            this.ExposureId = exposureId;
            this.ImageId = imageId;
        }
    }

    public interface IImageSaveWatcher {

        void Start();

        void WaitForExposure(string identifier, ExposureWaitData waitData);

        void Stop();
    }
}