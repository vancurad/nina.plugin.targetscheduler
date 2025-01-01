using NINA.Plugin.TargetScheduler.Database;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Plugin.TargetScheduler.Util;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;

namespace NINA.Plugin.TargetScheduler.Grading {

    public enum GradingStatus {
        Pending, Accepted, Rejected
    };

    public enum GradingResult {
        Accepted, Rejected_RMS, Rejected_Stars, Rejected_HFR, Rejected_FWHM, Rejected_Eccentricity, Exception
    };

    public class ImageGrader : IImageGrader {
        public static readonly string REJECTED_SUBDIR = "rejected";

        public static readonly string REJECT_RMS = "Guiding RMS";
        public static readonly string REJECT_STARS = "Star Count";
        public static readonly string REJECT_HFR = "HFR";
        public static readonly string REJECT_FWHM = "FWHM";
        public static readonly string REJECT_ECCENTRICITY = "Eccentricity";

        public void Grade(GradingWorkData workData) {
            if (workData.IsEmulated) {
                TSLogger.Debug("skipping image grading for emulation");
                return;
            }

            ExposurePlan exposurePlan = GetExposurePlan(workData.ExposurePlanId);
            Target target = GetTarget(workData.TargetId);
            string tag = $"target {target.Name}, expId={exposurePlan.Id}";

            try {
                TSLogger.Info($"starting image grading on {tag}");
                ImageGraderPreferences graderPreferences = workData.GraderPreferences;
                List<AcquiredImage> population = GetMatchingAcquired(workData, target, GetAllAcquired(exposurePlan));

                if (DelayedGradingEnabled(graderPreferences)) {
                    if (exposurePlan.Desired == 0) {
                        TSLogger.Warning("desired count on exposure plan is zero during grading - aborting");
                        return;
                    }

                    if (CurrentDelayThreshold(population.Count, exposurePlan.Desired) >= graderPreferences.DelayGradingThreshold) {
                        List<AcquiredImage> pending = population.Where(a => a.GradingStatus == GradingStatus.Pending).ToList();
                        TSLogger.Info($"delayed grading triggered for {tag}: acquired={population.Count}, desired={exposurePlan.Desired}, pending={pending.Count}");

                        pending.ForEach(acquiredImage => {
                            GradingResult result = GradeImage(exposurePlan, acquiredImage, workData, population);
                            UpdateDatabase(result, exposurePlan, acquiredImage);
                            if (graderPreferences.EnableMoveRejected && result != GradingResult.Accepted) {
                                MoveRejected(acquiredImage.Metadata.FileName);
                            }
                        });
                    } else {
                        TSLogger.Info($"delayed grading not yet triggered for {tag}");
                    }
                } else {
                    AcquiredImage acquiredImage = GetCurrentAcquired(workData.AcquiredImageId);
                    List<AcquiredImage> immediatePopulation = GetImmediatePopulation(graderPreferences.MaxGradingSampleSize, population);
                    GradingResult result = GradeImage(exposurePlan, acquiredImage, workData, immediatePopulation);
                    UpdateDatabase(result, exposurePlan, acquiredImage);
                    if (graderPreferences.EnableMoveRejected && result != GradingResult.Accepted) {
                        MoveRejected(acquiredImage.Metadata.FileName);
                    }
                }
            } catch (Exception ex) {
                TSLogger.Error($"exception during image grading: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private GradingResult GradeImage(ExposurePlan exposurePlan, AcquiredImage acquiredImage, GradingWorkData workData, List<AcquiredImage> population) {
            ImageGraderPreferences prefs = workData.GraderPreferences;
            GraderExpert expert = new GraderExpert(workData);

            try {
                if (!expert.EnableGradeRMS && expert.NoGradingMetricsEnabled) {
                    TSLogger.Info("image grading: no metrics enabled => accepted");
                    return GradingResult.Accepted;
                }

                if (!expert.GradeRMS()) {
                    TSLogger.Info("image grading: failed guiding RMS => NOT accepted");
                    return GradingResult.Rejected_RMS;
                }

                if (expert.NoGradingMetricsEnabled) {
                    TSLogger.Info("image grading: no additional metrics enabled => accepted");
                    return GradingResult.Accepted;
                }

                // If not delayed and we don't yet have enough images to compare against, assume acceptable
                if (!DelayedGradingEnabled(prefs) && population == null) {
                    TSLogger.Info("image grading: not enough matching images => accepted");
                    return GradingResult.Accepted;
                }

                if (!expert.GradeStars(population)) {
                    TSLogger.Info("image grading: failed detected star count grading => NOT accepted");
                    return GradingResult.Rejected_Stars;
                }

                if (!expert.GradeHFR(population)) {
                    TSLogger.Info("image grading: failed HFR grading => NOT accepted");
                    return GradingResult.Rejected_HFR;
                }

                if (!expert.GradeFWHM(population)) {
                    TSLogger.Info("image grading: failed FWHM grading => NOT accepted");
                    return GradingResult.Rejected_FWHM;
                }

                if (!expert.GradeEccentricity(population)) {
                    TSLogger.Info("image grading: failed eccentricity grading => NOT accepted");
                    return GradingResult.Rejected_Eccentricity;
                }

                TSLogger.Info("image grading: all tests passed => accepted");
                return GradingResult.Accepted;
            } catch (Exception e) {
                TSLogger.Error("image grading: exception => NOT accepted");
                TSLogger.Error(e);
                return GradingResult.Exception;
            }
        }

        public void MoveRejected(string fileLocalPath) {
            string dstDir = Path.Combine(Path.GetDirectoryName(fileLocalPath), REJECTED_SUBDIR);
            TSLogger.Info($"moving rejected image to {dstDir}");
            Utils.MoveFile(fileLocalPath, dstDir);
        }

        public virtual void UpdateDatabase(GradingResult result, ExposurePlan exposurePlan, AcquiredImage acquiredImage) {
            using (var context = new SchedulerDatabaseInteraction().GetContext()) {
                using (var transaction = context.Database.BeginTransaction()) {
                    try {
                        if (result == GradingResult.Accepted) {
                            exposurePlan.Accepted++;
                            context.ExposurePlanSet.AddOrUpdate(exposurePlan);
                        }

                        acquiredImage.GradingStatus = GradingResultToStatus(result);
                        acquiredImage.RejectReason = GradingResultToReason(result);
                        context.AcquiredImageSet.AddOrUpdate(acquiredImage);

                        context.SaveChanges();
                        transaction.Commit();
                    } catch (Exception e) {
                        TSLogger.Error($"exception updating database for graded image: {e.Message}\n{e.StackTrace}");
                        SchedulerDatabaseContext.CheckValidationErrors(e);
                    }
                }
            }
        }

        public GradingStatus GradingResultToStatus(GradingResult result) {
            return result == GradingResult.Accepted ? GradingStatus.Accepted : GradingStatus.Rejected;
        }

        public string GradingResultToReason(GradingResult result) {
            switch (result) {
                case GradingResult.Accepted: return string.Empty;
                case GradingResult.Rejected_RMS: return REJECT_RMS;
                case GradingResult.Rejected_Stars: return REJECT_STARS;
                case GradingResult.Rejected_HFR: return REJECT_HFR;
                case GradingResult.Rejected_FWHM: return REJECT_FWHM;
                case GradingResult.Rejected_Eccentricity: return REJECT_ECCENTRICITY;
                default: return "?";
            }
        }

        public virtual ExposurePlan GetExposurePlan(int exposurePlanId) {
            using (var context = new SchedulerDatabaseInteraction().GetContext()) {
                return context.GetExposurePlan(exposurePlanId);
            }
        }

        public virtual Target GetTarget(int targetId) {
            using (var context = new SchedulerDatabaseInteraction().GetContext()) {
                return context.GetTargetOnly(targetId);
            }
        }

        public virtual AcquiredImage GetCurrentAcquired(int id) {
            using (var context = new SchedulerDatabaseInteraction().GetContext()) {
                return context.GetAcquiredImage(id);
            }
        }

        private List<AcquiredImage> GetMatchingAcquired(GradingWorkData workData, Target target, List<AcquiredImage> acquiredImages) {
            if (Common.IsEmpty(acquiredImages)) {
                return new List<AcquiredImage>(Array.Empty<AcquiredImage>());
            }

            ImageSavedEventArgs imageSavedEventArgs = workData.ImageSavedEventArgs;

            return acquiredImages.Where(a =>
                a.ProfileId == workData.GraderPreferences.Profile.Id.ToString() &&
                a.Metadata.ExposureDuration == imageSavedEventArgs.Duration &&
                a.Metadata.Gain == imageSavedEventArgs.MetaData.Camera.Gain &&
                a.Metadata.Offset == imageSavedEventArgs.MetaData.Camera.Offset &&
                a.Metadata.Binning == imageSavedEventArgs.MetaData.Image.Binning &&
                a.Metadata.ROI == target.ROI
            ).ToList();
        }

        public virtual List<AcquiredImage> GetAllAcquired(ExposurePlan exposurePlan) {
            using (var context = new SchedulerDatabaseInteraction().GetContext()) {
                return context.GetAcquiredImagesForGrading(exposurePlan);
            }
        }

        private List<AcquiredImage> GetImmediatePopulation(int maxSampleSize, List<AcquiredImage> population) {
            if (population.Count < 3) { return null; }
            return population.Take(maxSampleSize).ToList();
        }

        private bool DelayedGradingEnabled(ImageGraderPreferences graderPreferences) {
            return graderPreferences.DelayGradingThreshold > 0;
        }

        private double CurrentDelayThreshold(int count, int desired) {
            return (double)count / (double)desired;
        }
    }

    public interface IImageGrader {

        void Grade(GradingWorkData workPackage);
    }

    public class ImageGraderPreferences {
        public IProfile Profile { get; private set; }
        public double DelayGradingThreshold { get; private set; }
        public int MaxGradingSampleSize { get; private set; }
        public bool AcceptImprovement { get; private set; }
        public bool EnableGradeRMS { get; private set; }
        public double RMSPixelThreshold { get; private set; }
        public bool EnableGradeStars { get; private set; }
        public double DetectedStarsSigmaFactor { get; private set; }
        public bool EnableGradeHFR { get; private set; }
        public double HFRSigmaFactor { get; private set; }
        public bool EnableGradeFWHM { get; private set; }
        public double FWHMSigmaFactor { get; private set; }
        public bool EnableGradeEccentricity { get; private set; }
        public double EccentricitySigmaFactor { get; private set; }
        public bool EnableMoveRejected { get; private set; }

        public bool IsDelayEnabled { get { return DelayGradingThreshold > 0; } }

        public ImageGraderPreferences(IProfile profile, ProfilePreference profilePreference) {
            Profile = profile;
            DelayGradingThreshold = profilePreference.DelayGrading;
            MaxGradingSampleSize = profilePreference.MaxGradingSampleSize;
            AcceptImprovement = profilePreference.AcceptImprovement;
            EnableGradeRMS = profilePreference.EnableGradeRMS;
            RMSPixelThreshold = profilePreference.RMSPixelThreshold;
            EnableGradeStars = profilePreference.EnableGradeStars;
            DetectedStarsSigmaFactor = profilePreference.DetectedStarsSigmaFactor;
            EnableGradeHFR = profilePreference.EnableGradeHFR;
            HFRSigmaFactor = profilePreference.HFRSigmaFactor;
            EnableGradeFWHM = profilePreference.EnableGradeFWHM;
            FWHMSigmaFactor = profilePreference.FWHMSigmaFactor;
            EnableGradeEccentricity = profilePreference.EnableGradeEccentricity;
            EccentricitySigmaFactor = profilePreference.EccentricitySigmaFactor;
            EnableMoveRejected = profilePreference.EnableMoveRejected;
        }

        public ImageGraderPreferences(IProfile profile, double DelayGradingThreshold,
                                    int MaxGradingSampleSize, bool AcceptImprovement,
                                    bool EnableGradeRMS, double RMSPixelThreshold,
                                    bool EnableGradeStars, double DetectedStarsSigmaFactor,
                                    bool EnableGradeHFR, double HFRSigmaFactor,
                                    bool EnableGradeFWHM, double FWHMSigmaFactor,
                                    bool EnableGradeEccentricity, double EccentricitySigmaFactor,
                                    bool EnableMoveRejected = false) {
            this.Profile = profile;
            this.DelayGradingThreshold = DelayGradingThreshold;
            this.MaxGradingSampleSize = MaxGradingSampleSize;
            this.AcceptImprovement = AcceptImprovement;
            this.EnableGradeRMS = EnableGradeRMS;
            this.RMSPixelThreshold = RMSPixelThreshold;
            this.EnableGradeStars = EnableGradeStars;
            this.DetectedStarsSigmaFactor = DetectedStarsSigmaFactor;
            this.EnableGradeHFR = EnableGradeHFR;
            this.HFRSigmaFactor = HFRSigmaFactor;
            this.EnableGradeFWHM = EnableGradeFWHM;
            this.FWHMSigmaFactor = FWHMSigmaFactor;
            this.EnableGradeEccentricity = EnableGradeEccentricity;
            this.EccentricitySigmaFactor = EccentricitySigmaFactor;
            this.EnableMoveRejected = EnableMoveRejected;
        }
    }
}