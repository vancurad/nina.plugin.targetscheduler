﻿using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Profile.Interfaces;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning {

    /// <summary>
    /// To estimate what the planner might do on a given night, a series of plans can be generated by repeatedly
    /// calling the planner using the end time of the previous run as the next starting point.  This series is 'perfect'
    /// for two reasons.  One, it assumes that operations that absolutely will take time (like slew/center, switching filters,
    /// autofocus, meridian flips, etc) take zero time.  And two, all images are assumed to be acceptable and will
    /// increment the accepted count for the target/filter.  The net result is that acceptable images will be acquired
    /// (and projects completed) significantly faster than in actual usage.
    ///
    /// Nevertheless, a perfect plan provides some idea of what the planner will do on a given night which is useful
    /// for previewing and troubleshooting.
    /// </summary>
    public class PreviewPlanner {

        public PreviewPlanner() {
        }

        public List<SchedulerPlan> GetPlanPreview(DateTime atTime, IProfileService profileService, ProfilePreference profilePreferences, List<IProject> projects) {
            TSLogger.Info("-- BEGIN PLAN PREVIEW ----------------------------------------------------------");

            List<SchedulerPlan> plans = new List<SchedulerPlan>();
            DateTime currentTime = atTime;
            ITarget previousPlanTarget = null;

            try {
                SchedulerPlan plan;
                while ((plan = new Planner(currentTime, profileService.ActiveProfile, profilePreferences, false, projects).GetPlan(previousPlanTarget)) != null) {
                    plans.Add(plan);
                    previousPlanTarget = plan.IsWait ? null : plan.PlanTarget;
                    currentTime = plan.IsWait ? (DateTime)plan.WaitForNextTargetTime : plan.EndTime;
                    PrepForNextRun(projects, plan);
                }

                return plans;
            } catch (Exception ex) {
                TSLogger.Error($"exception during plan preview: {ex.Message}\n{ex.StackTrace}");
                return plans;
            } finally {
                TSLogger.Info("-- END PLAN PREVIEW ------------------------------------------------------------");
            }
        }

        private void PrepForNextRun(List<IProject> projects, SchedulerPlan plan) {
            if (!plan.IsWait)
                plan.PlanTarget.FilterCadence.Advance();

            foreach (IProject project in projects) {
                project.Rejected = false;
                project.RejectedReason = null;
                foreach (ITarget target in project.Targets) {
                    target.ScoringResults = null;
                    target.Rejected = false;
                    target.RejectedReason = null;

                    foreach (IExposure exposure in target.ExposurePlans) {
                        if (project.EnableGrader) {
                            exposure.Accepted += exposure.PlannedExposures;
                        } else {
                            exposure.Acquired += exposure.PlannedExposures;
                        }

                        exposure.PlannedExposures = 0;
                        exposure.Rejected = false;
                        exposure.RejectedReason = null;
                        exposure.MoonAvoidanceScore = MoonAvoidanceExpert.SCORE_OFF;
                    }
                }
            }
        }
    }
}