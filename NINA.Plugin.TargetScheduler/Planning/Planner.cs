using NINA.Astrometry;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Plugin.TargetScheduler.Astrometry;
using NINA.Plugin.TargetScheduler.Database;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Plugin.TargetScheduler.Util;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class Planner {
        private bool checkCondition = false;
        private DateTime atTime;
        private IProfile activeProfile;
        private ProfilePreference profilePreferences;
        private ObserverInfo observerInfo;
        private List<IProject> projects;

        public Planner(DateTime atTime, IProfile profile, ProfilePreference profilePreferences, bool checkCondition)
            : this(atTime, profile, profilePreferences, checkCondition, null) { }

        public Planner(DateTime atTime, IProfile profile, ProfilePreference profilePreferences, bool checkCondition, List<IProject> projects) {
            this.atTime = atTime;
            this.activeProfile = profile;
            this.profilePreferences = profilePreferences;
            this.checkCondition = checkCondition;
            this.projects = projects;
            this.observerInfo = new ObserverInfo {
                Latitude = activeProfile.AstrometrySettings.Latitude,
                Longitude = activeProfile.AstrometrySettings.Longitude,
                Elevation = activeProfile.AstrometrySettings.Elevation,
            };
        }

        public SchedulerPlan GetPlan(ITarget previousTarget) {
            string type = checkCondition ? "CONDITION" : "EXECUTE";
            string title = $"PLANNING ENGINE RUN ({type})";

            TSLogger.Info($"-- BEGIN {title} ---------------------------------------------------");
            TSLogger.Debug($"getting current plan for {Utils.FormatDateTimeFull(atTime)}");

            /*
            if (Common.USE_EMULATOR) {
                Notification.ShowInformation("REMINDER: running plan emulation");
                TSLogger.Info($"-- END {title} -----------------------------------------------------");
                return new PlannerEmulator(atTime, activeProfile).GetPlan(previousPlanTarget);
            }*/

            using (MyStopWatch.Measure("Scheduler Plan Generation")) {
                try {
                    if (projects == null) {
                        projects = GetProjects();
                    }

                    projects = FilterForIncomplete(projects);
                    projects = FilterForVisibility(projects);
                    projects = FilterForMoonAvoidance(projects);

                    return null; // FIXME
                } catch (Exception ex) {
                    if (ex is SequenceEntityFailedException) {
                        throw;
                    }

                    TSLogger.Error($"exception generating plan: {ex.StackTrace}");
                    throw new SequenceEntityFailedException($"Scheduler: exception generating plan: {ex.Message}", ex);
                } finally {
                    TSLogger.Info($"-- END {title} -----------------------------------------------------");
                }
            }
        }

        private List<IProject> GetProjects() {
            try {
                SchedulerDatabaseInteraction database = new SchedulerDatabaseInteraction();
                SchedulerPlanLoader loader = new SchedulerPlanLoader(activeProfile);
                return loader.LoadActiveProjects(database.GetContext());
            } catch (Exception ex) {
                TSLogger.Error($"exception reading database: {ex.StackTrace}");
                throw new SequenceEntityFailedException($"Scheduler: exception reading database: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Review the project list and reject those projects that are already complete.
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        public List<IProject> FilterForIncomplete(List<IProject> projects) {
            if (NoProjects(projects)) { return null; }

            foreach (IProject planProject in projects) {
                if (!ProjectIsInComplete(planProject)) {
                    SetRejected(planProject, Reasons.ProjectComplete);
                    foreach (ITarget planTarget in planProject.Targets) {
                        SetRejected(planTarget, Reasons.TargetComplete);
                    }
                }
            }

            return PropagateRejections(projects);
        }

        /// <summary>
        /// Review each project and the list of associated targets: reject those targets that are not visible.  If all targets
        /// for the project are rejected, mark the project rejected too.  A target is visible if it is above the horizon
        /// within the time window set by the most inclusive twilight over all incomplete exposure plans for that target, is clipped
        /// to any meridian window, and the remaining visible time is greater than the minimum imaging time preference for the project.
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        public List<IProject> FilterForVisibility(List<IProject> projects) {
            if (NoProjects(projects)) { return null; }

            foreach (IProject planProject in projects) {
                if (planProject.Rejected) { continue; }

                foreach (ITarget planTarget in planProject.Targets) {
                    if (planTarget.Rejected) { continue; }

                    if (!AstrometryUtils.RisesAtLocation(observerInfo, planTarget.Coordinates)) {
                        TSLogger.Warning($"target {planProject.Name}/{planTarget.Name} never rises at location - skipping");
                        SetRejected(planTarget, Reasons.TargetNeverRises);
                        continue;
                    }

                    // Get the most inclusive twilight over all incomplete exposure plans
                    TwilightCircumstances nighttimeCircumstances = TwilightCircumstances.AdjustTwilightCircumstances(observerInfo, atTime);
                    TimeInterval twilightSpan = nighttimeCircumstances.GetTwilightSpan(GetOverallTwilight(planTarget));

                    // At high latitudes near the summer solsice, you can lose nighttime completely (even below the polar circle)
                    if (twilightSpan == null) {
                        TSLogger.Warning($"No twilight span for target {planProject.Name}/{planTarget.Name} on {Utils.FormatDateTimeFull(atTime)} at latitude {observerInfo.Latitude}");
                        SetRejected(planTarget, Reasons.TargetAllExposurePlans);
                        continue;
                    }

                    TargetVisibility targetVisibility = new TargetVisibility(
                        planTarget,
                        observerInfo,
                        nighttimeCircumstances.OnDate,
                        nighttimeCircumstances.Sunset,
                        nighttimeCircumstances.Sunrise);

                    if (!targetVisibility.ImagingPossible) {
                        TSLogger.Debug($"Target not visible at all {planProject.Name}/{planTarget.Name} on {Utils.FormatDateTimeFull(atTime)} at latitude {observerInfo.Latitude}");
                        SetRejected(planTarget, Reasons.TargetNotVisible);
                        continue;
                    }

                    // Determine the next time interval of visibility of at least the mimimum time
                    VisibilityDetermination viz = targetVisibility.NextVisibleInterval(atTime, twilightSpan, planProject.HorizonDefinition, planProject.MinimumTime * 60);
                    if (!viz.IsVisible) {
                        TSLogger.Debug($"Target not visible for rest of night {planProject.Name}/{planTarget.Name} on {Utils.FormatDateTimeFull(atTime)} at latitude {observerInfo.Latitude}");
                        SetRejected(planTarget, Reasons.TargetNotVisible);
                        continue;
                    }

                    DateTime targetStartTime = viz.StartTime;
                    DateTime targetTransitTime = targetVisibility.TransitTime;
                    DateTime targetEndTime = viz.StopTime;

                    // Clip time span to optional meridian window
                    TimeInterval meridianClippedSpan = null;
                    if (planProject.MeridianWindow > 0) {
                        TSLogger.Debug($"checking meridian window for {planProject.Name}/{planTarget.Name}");
                        meridianClippedSpan = new MeridianWindowClipper().Clip(
                                           targetStartTime,
                                           targetTransitTime,
                                           targetEndTime,
                                           planProject.MeridianWindow);

                        if (meridianClippedSpan == null) {
                            SetRejected(planTarget, Reasons.TargetMeridianWindowClipped);
                            continue;
                        }

                        planTarget.MeridianWindow = meridianClippedSpan;
                        targetStartTime = meridianClippedSpan.StartTime;
                        targetEndTime = meridianClippedSpan.EndTime;
                    }

                    // If the start time is in the future, reject ... for now
                    DateTime actualStart = atTime > targetStartTime ? atTime : targetStartTime;
                    if (actualStart > atTime) {
                        planTarget.StartTime = actualStart;
                        planTarget.EndTime = targetEndTime;
                        string reason = meridianClippedSpan != null ? Reasons.TargetBeforeMeridianWindow : Reasons.TargetNotYetVisible;
                        SetRejected(planTarget, reason);
                        continue;
                    }

                    // Otherwise the target is a candidate
                    planTarget.StartTime = targetStartTime;
                    planTarget.EndTime = targetEndTime;
                    planTarget.CulminationTime = targetTransitTime;
                }
            }

            return PropagateRejections(projects);
        }

        /// <summary>
        /// Review each project and the list of associated targets.  For each filter plan where moon avoidance is enabled,
        /// calculate the avoidance criteria and reject as needed.
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        public List<IProject> FilterForMoonAvoidance(List<IProject> projects) {
            if (NoProjects(projects)) { return null; }
            MoonAvoidanceExpert expert = new MoonAvoidanceExpert(observerInfo);

            foreach (IProject planProject in projects) {
                if (planProject.Rejected) { continue; }

                foreach (ITarget planTarget in planProject.Targets) {
                    if (planTarget.Rejected && planTarget.RejectedReason != Reasons.TargetNotYetVisible) { continue; }

                    foreach (IExposure planExposure in planTarget.ExposurePlans) {
                        if (planExposure.IsIncomplete()) {
                            if (expert.IsRejected(atTime, planTarget, planExposure)) {
                                SetRejected(planExposure, Reasons.FilterMoonAvoidance);
                            }
                        }
                    }
                }
            }

            return PropagateRejections(projects);
        }

        private bool NoProjects(List<IProject> projects) {
            return projects == null || projects.Count == 0;
        }

        private void SetRejected(IProject planProject, string reason) {
            planProject.Rejected = true;
            planProject.RejectedReason = reason;
        }

        private void SetRejected(ITarget planTarget, string reason) {
            planTarget.Rejected = true;
            planTarget.RejectedReason = reason;
        }

        private void SetRejected(IExposure planExposure, string reason) {
            planExposure.Rejected = true;
            planExposure.RejectedReason = reason;
        }

        private List<IProject> PropagateRejections(List<IProject> projects) {
            if (NoProjects(projects)) { return null; }

            foreach (IProject planProject in projects) {
                if (planProject.Rejected) { continue; }
                bool projectRejected = true;

                foreach (ITarget planTarget in planProject.Targets) {
                    if (planTarget.Rejected) { continue; }
                    bool targetRejected = true;

                    bool allExposurePlansComplete = true;
                    foreach (IExposure planExposure in planTarget.ExposurePlans) {
                        if (!planExposure.Rejected) {
                            targetRejected = false;
                            break;
                        }

                        if (planExposure.Rejected && planExposure.RejectedReason != Reasons.FilterComplete) {
                            allExposurePlansComplete = false;
                        }
                    }

                    if (targetRejected) {
                        SetRejected(planTarget, allExposurePlansComplete ? Reasons.TargetComplete : Reasons.TargetAllExposurePlans);
                    }

                    if (!planTarget.Rejected) {
                        projectRejected = false;
                    }
                }

                if (projectRejected) {
                    SetRejected(planProject, Reasons.ProjectAllTargets);
                }
            }

            return projects;
        }

        private bool ProjectIsInComplete(IProject planProject) {
            bool incomplete = false;

            foreach (ITarget target in planProject.Targets) {
                foreach (IExposure planExposure in target.ExposurePlans) {
                    if (planExposure.NeededExposures() > 0) {
                        incomplete = true;
                    } else {
                        SetRejected(planExposure, Reasons.FilterComplete);
                    }
                }
            }

            return incomplete;
        }

        private TwilightLevel GetOverallTwilight(ITarget planTarget) {
            TwilightLevel twilightLevel = TwilightLevel.Nighttime;
            foreach (IExposure planExposure in planTarget.ExposurePlans) {
                // find most permissive (brightest) twilight over all incomplete plans
                if (planExposure.TwilightLevel > twilightLevel && planExposure.IsIncomplete()) {
                    twilightLevel = planExposure.TwilightLevel;
                }
            }

            return twilightLevel;
        }
    }

    public class Reasons {
        public const string ProjectComplete = "complete";
        public const string ProjectNoVisibleTargets = "no visible targets";
        public const string ProjectMoonAvoidance = "moon avoidance";
        public const string ProjectAllTargets = "all targets rejected";

        public const string TargetComplete = "complete";
        public const string TargetNeverRises = "never rises at location";
        public const string TargetNotVisible = "not visible";
        public const string TargetNotYetVisible = "not yet visible";
        public const string TargetMeridianWindowClipped = "clipped by meridian window";
        public const string TargetBeforeMeridianWindow = "before meridian window";
        public const string TargetMoonAvoidance = "moon avoidance";
        public const string TargetLowerScore = "lower score";
        public const string TargetAllExposurePlans = "all exposure plans rejected";

        public const string FilterComplete = "complete";
        public const string FilterMoonAvoidance = "moon avoidance";
        public const string FilterNoExposuresPlanned = "no exposures planned";

        private Reasons() {
        }
    }
}