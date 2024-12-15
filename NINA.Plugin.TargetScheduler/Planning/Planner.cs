using NINA.Astrometry;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Plugin.TargetScheduler.Astrometry;
using NINA.Plugin.TargetScheduler.Database;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Exposures;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Planning.Scoring;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Plugin.TargetScheduler.Util;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class Planner {
        private const int TARGET_VISIBILITY_SAMPLE_INTERVAL = 10;

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

                    // Filter all targets for suitability
                    projects = FilterForIncomplete(projects);
                    projects = FilterForVisibility(projects);
                    projects = FilterForMoonAvoidance(projects);
                    projects = FilterForTwilight(projects);

                    // See if one or more targets are ready to image now
                    List<ITarget> readyTargets = GetTargetsReadyNow(projects);
                    if (readyTargets.Count > 0) {
                        SelectTargetExposures(readyTargets, previousTarget);

                        // If only one ready target, no need to run scoring engine
                        ITarget selectedTarget;
                        if (readyTargets.Count == 1) {
                            selectedTarget = readyTargets[0];
                        } else {
                            selectedTarget = SelectTargetByScore(readyTargets, new ScoringEngine(activeProfile, profilePreferences, atTime, previousTarget));
                        }

                        // Generate instructions for the selected target/exposure
                        List<IInstruction> instructions = new InstructionGenerator().Generate(selectedTarget, previousTarget);
                        return new SchedulerPlan(atTime, projects, selectedTarget, instructions, !checkCondition);
                    } else {
                        // TODO: check for wait ...
                        // HERE
                        TSLogger.Info("Scheduler Planner: no target selected");
                        return null;

                        //throw new Exception("wait not yet implemented");
                        /* Determine if we can wait for a target:
                         * - For each remaining target:
                         *   - For each visibility span:
                         *     - At X second intervals over the span, check if any filters pass moon avoidance.  If so, that's
                         *       the effective start time for this project.  Don't forget meridian clipping!
                         * - The soonest effective start time over all targets is our wait time.
                         * - Otherwise, we're done - nothing else available tonight.
                         */
                    }
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

        /// <summary>
        /// Review the project list and reject those projects that are already complete.
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        public List<IProject> FilterForIncomplete(List<IProject> projects) {
            if (NoProjects(projects)) { return null; }

            foreach (IProject project in projects) {
                if (!ProjectIsInComplete(project)) {
                    SetRejected(project, Reasons.ProjectComplete);
                    foreach (ITarget target in project.Targets) {
                        SetRejected(target, Reasons.TargetComplete);
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

            foreach (IProject project in projects) {
                if (project.Rejected) { continue; }

                foreach (ITarget target in project.Targets) {
                    if (target.Rejected) { continue; }

                    if (!AstrometryUtils.RisesAtLocation(observerInfo, target.Coordinates)) {
                        TSLogger.Warning($"target {project.Name}/{target.Name} never rises at location - skipping");
                        SetRejected(target, Reasons.TargetNeverRises);
                        continue;
                    }

                    // Get the most inclusive twilight over all incomplete exposure plans
                    TwilightCircumstances twilightCircumstances = TwilightCircumstances.AdjustTwilightCircumstances(observerInfo, atTime);
                    TimeInterval twilightSpan = twilightCircumstances.GetTwilightSpan(GetOverallTwilight(target));

                    // At high latitudes near the summer solsice, you can lose nighttime completely (even below the polar circle)
                    if (twilightSpan == null) {
                        TSLogger.Warning($"No twilight span for target {project.Name}/{target.Name} on {Utils.FormatDateTimeFull(atTime)} at latitude {observerInfo.Latitude}");
                        SetRejected(target, Reasons.TargetAllExposurePlans);
                        continue;
                    }

                    TargetVisibility targetVisibility = new TargetVisibility(
                        target,
                        observerInfo,
                        twilightCircumstances.OnDate,
                        twilightCircumstances.Sunset,
                        twilightCircumstances.Sunrise,
                        TARGET_VISIBILITY_SAMPLE_INTERVAL);

                    if (!targetVisibility.ImagingPossible) {
                        TSLogger.Debug($"Target not visible at all {project.Name}/{target.Name} on {Utils.FormatDateTimeFull(atTime)} at latitude {observerInfo.Latitude}");
                        SetRejected(target, Reasons.TargetNotVisible);
                        continue;
                    }

                    // Determine the next time interval of visibility of at least the mimimum time
                    VisibilityDetermination viz = targetVisibility.NextVisibleInterval(atTime, twilightSpan, project.HorizonDefinition, project.MinimumTime * 60);
                    if (!viz.IsVisible) {
                        TSLogger.Debug($"Target not visible for rest of night {project.Name}/{target.Name} on {Utils.FormatDateTimeFull(atTime)} at latitude {observerInfo.Latitude}");
                        SetRejected(target, Reasons.TargetNotVisible);
                        continue;
                    }

                    DateTime targetStartTime = viz.StartTime;
                    DateTime targetTransitTime = targetVisibility.TransitTime;
                    DateTime targetEndTime = viz.StopTime;

                    // Clip time span to optional meridian window
                    TimeInterval meridianClippedSpan = null;
                    if (project.MeridianWindow > 0) {
                        TSLogger.Debug($"checking meridian window for {project.Name}/{target.Name}");
                        meridianClippedSpan = new MeridianWindowClipper().Clip(
                                           targetStartTime,
                                           targetTransitTime,
                                           targetEndTime,
                                           project.MeridianWindow);

                        if (meridianClippedSpan == null) {
                            SetRejected(target, Reasons.TargetMeridianWindowClipped);
                            continue;
                        }

                        target.MeridianWindow = meridianClippedSpan;
                        targetStartTime = meridianClippedSpan.StartTime;
                        targetEndTime = meridianClippedSpan.EndTime;
                    }

                    // If the start time is in the future, reject ... for now
                    DateTime actualStart = atTime > targetStartTime ? atTime : targetStartTime;
                    if (actualStart > atTime) {
                        target.StartTime = actualStart;
                        target.EndTime = targetEndTime;
                        string reason = meridianClippedSpan != null ? Reasons.TargetBeforeMeridianWindow : Reasons.TargetNotYetVisible;
                        SetRejected(target, reason);
                        continue;
                    }

                    // Otherwise the target is a candidate
                    target.StartTime = targetStartTime;
                    target.EndTime = targetEndTime;
                    target.CulminationTime = targetTransitTime;
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

            foreach (IProject project in projects) {
                if (project.Rejected) { continue; }

                foreach (ITarget target in project.Targets) {
                    if (target.Rejected && target.RejectedReason != Reasons.TargetNotYetVisible) { continue; }

                    foreach (IExposure exposure in target.ExposurePlans) {
                        if (exposure.IsIncomplete()) {
                            if (expert.IsRejected(atTime, target, exposure)) {
                                SetRejected(exposure, Reasons.FilterMoonAvoidance);
                            }
                        }
                    }
                }
            }

            return PropagateRejections(projects);
        }

        /// <summary>
        /// Reject target exposures that are not suitable for the current level of twilight.
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<IProject> FilterForTwilight(List<IProject> projects) {
            if (NoProjects(projects)) { return null; }
            TwilightCircumstances twilightCircumstances = TwilightCircumstances.AdjustTwilightCircumstances(observerInfo, atTime);
            TwilightLevel? currentTwilightLevel = twilightCircumstances.GetCurrentTwilightLevel(atTime);

            foreach (IProject project in projects) {
                if (project.Rejected) { continue; }

                foreach (ITarget target in project.Targets) {
                    if (target.Rejected) { continue; }

                    foreach (IExposure exposure in target.ExposurePlans) {
                        if (!exposure.Rejected && exposure.IsIncomplete()) {
                            if (currentTwilightLevel.HasValue) {
                                if (currentTwilightLevel > exposure.TwilightLevel)
                                    SetRejected(exposure, Reasons.FilterTwilight);
                            } else {
                                SetRejected(exposure, Reasons.FilterTwilight);
                            }
                        }
                    }
                }
            }

            return PropagateRejections(projects);
        }

        /// <summary>
        /// Return the list of targets that are imagable now.
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        public List<ITarget> GetTargetsReadyNow(List<IProject> projects) {
            List<ITarget> targets = new List<ITarget>();

            if (NoProjects(projects)) { return targets; }

            foreach (IProject project in projects) {
                if (project.Rejected) { continue; }

                foreach (ITarget target in project.Targets) {
                    if (target.Rejected) { continue; }

                    TimeSpan diff = atTime - target.StartTime;
                    if (Math.Abs(diff.TotalSeconds) <= TARGET_VISIBILITY_SAMPLE_INTERVAL * 2) {
                        targets.Add(target);
                    }
                }
            }

            return targets;
        }

        /// <summary>
        /// Select the best exposure plan now for each potential target.
        /// </summary>
        /// <param name="readyTargets"></param>
        /// <param name="previousTarget"></param>
        public void SelectTargetExposures(List<ITarget> readyTargets, ITarget previousTarget) {
            if (Common.IsEmpty(readyTargets)) {
                return;
            }

            ExposureSelectionExpert selectionExpert = new ExposureSelectionExpert();
            IExposure previousExposure = previousTarget != null ? previousTarget.SelectedExposure : null;

            foreach (ITarget target in readyTargets) {
                IExposureSelector exposureSelector = selectionExpert.GetExposureSelector(target.Project, target);
                target.SelectedExposure = exposureSelector.Select(atTime, target.Project, target, previousExposure);
            }
        }

        /// <summary>
        /// Run the scoring engine, applying the weighted rules to determine the target with the highest score.
        /// </summary>
        /// <param name="readyTargets"></param>
        /// <param name="scoringEngine"></param>
        /// <returns></returns>
        public ITarget SelectTargetByScore(List<ITarget> readyTargets, IScoringEngine scoringEngine) {
            if (Common.IsEmpty(readyTargets)) {
                throw new ArgumentException("no ready targets in SelectTargetByScore");
            }

            if (readyTargets.Count == 1) { return readyTargets[0]; }

            ITarget highScoreTarget = null;
            double highScore = double.MinValue;

            foreach (ITarget target in readyTargets) {
                TSLogger.Debug($"running scoring engine for project/target {target.Project.Name}/{target.Name}");
                scoringEngine.RuleWeights = target.Project.RuleWeights;
                double score = scoringEngine.ScoreTarget(target);
                if (score > highScore) {
                    highScoreTarget = target;
                    highScore = score;
                }
            }

            // Mark losing targets rejected
            foreach (ITarget target in readyTargets) {
                if (target != highScoreTarget) {
                    target.Rejected = true;
                    target.RejectedReason = Reasons.TargetLowerScore;
                }
            }

            return highScoreTarget;
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

        private List<ITarget> GetActiveTargets(List<IProject> projects) {
            List<ITarget> targets = new List<ITarget>();

            if (NoProjects(projects)) {
                return targets;
            }

            foreach (IProject project in projects) {
                if (project.Rejected) { continue; }
                foreach (ITarget target in project.Targets) {
                    if (target.Rejected) { continue; }
                    targets.Add(target);
                }
            }

            return targets;
        }

        private bool NoProjects(List<IProject> projects) {
            return projects == null || projects.Count == 0;
        }

        private void SetRejected(IProject project, string reason) {
            project.Rejected = true;
            project.RejectedReason = reason;
        }

        private void SetRejected(ITarget target, string reason) {
            target.Rejected = true;
            target.RejectedReason = reason;
        }

        private void SetRejected(IExposure exposure, string reason) {
            exposure.Rejected = true;
            exposure.RejectedReason = reason;
        }

        private List<IProject> PropagateRejections(List<IProject> projects) {
            if (NoProjects(projects)) { return null; }

            foreach (IProject project in projects) {
                if (project.Rejected) { continue; }
                bool projectRejected = true;

                foreach (ITarget target in project.Targets) {
                    if (target.Rejected) { continue; }
                    bool targetRejected = true;

                    bool allExposurePlansComplete = true;
                    foreach (IExposure exposure in target.ExposurePlans) {
                        if (!exposure.Rejected) {
                            targetRejected = false;
                            break;
                        }

                        if (exposure.Rejected && exposure.RejectedReason != Reasons.FilterComplete) {
                            allExposurePlansComplete = false;
                        }
                    }

                    if (targetRejected) {
                        SetRejected(target, allExposurePlansComplete ? Reasons.TargetComplete : Reasons.TargetAllExposurePlans);
                    }

                    if (!target.Rejected) {
                        projectRejected = false;
                    }
                }

                if (projectRejected) {
                    SetRejected(project, Reasons.ProjectAllTargets);
                }
            }

            return projects;
        }

        private bool ProjectIsInComplete(IProject project) {
            bool incomplete = false;

            foreach (ITarget target in project.Targets) {
                foreach (IExposure exposure in target.ExposurePlans) {
                    if (exposure.NeededExposures() > 0) {
                        incomplete = true;
                    } else {
                        SetRejected(exposure, Reasons.FilterComplete);
                    }
                }
            }

            return incomplete;
        }

        private TwilightLevel GetOverallTwilight(ITarget target) {
            TwilightLevel twilightLevel = TwilightLevel.Nighttime;
            foreach (IExposure exposure in target.ExposurePlans) {
                // find most permissive (brightest) twilight over all incomplete plans
                if (exposure.TwilightLevel > twilightLevel && exposure.IsIncomplete()) {
                    twilightLevel = exposure.TwilightLevel;
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
        public const string FilterTwilight = "twilight";
        public const string FilterNoExposuresPlanned = "no exposures planned";

        private Reasons() {
        }
    }
}