﻿using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Select the next exposure based on the persisted filter cadence for this target.  Add a dither before the
    /// exposure if appropriate.
    /// </summary>
    public class BasicExposureSelector : BaseExposureSelector, IExposureSelector {

        public BasicExposureSelector() : base() {
        }

        public IExposure Select(DateTime atTime, IProject project, ITarget target, IExposure previousExposure) {
            FilterCadence filterCadence = target.FilterCadence;
            bool preDither = false;

            if (AllExposurePlansRejected(target)) {
                throw new Exception($"unexpected: all exposure plans were rejected at exposure selection time for target '{target.Name}' at time {atTime}");
            }

            if (filterCadence == null || filterCadence.Count == 0) {
                throw new Exception($"unexpected: empty filter cadence for target '{target.Name}' at time {atTime}");
            }

            bool exposureSkipped = false;
            foreach (IFilterCadenceItem item in filterCadence) {
                if (item.Action == FilterCadenceAction.Dither) {
                    preDither = true;
                    continue;
                }

                IExposure exposurePlan = target.ExposurePlans[item.ReferenceIdx];
                if (!exposurePlan.Rejected) {
                    exposurePlan.PreDither = DitherRequired(preDither, exposureSkipped, exposurePlan, previousExposure); ;
                    exposurePlan.PlannedExposures = 1;
                    filterCadence.SetLastSelected(item);
                    return exposurePlan;
                } else {
                    exposureSkipped = true;
                }
            }

            // Fail safe ... should not happen
            string msg = $"no acceptable exposure plan in basic exposure selector for target '{target.Name}' at time {atTime}";
            TSLogger.Error(msg);
            throw new Exception(msg);
        }
    }
}