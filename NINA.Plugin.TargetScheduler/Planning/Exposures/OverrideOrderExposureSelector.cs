﻿using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Select the next exposure based on the override exposure order for the target.
    /// </summary>
    public class OverrideOrderExposureSelector : BaseExposureSelector, IExposureSelector {

        public OverrideOrderExposureSelector(IProject project, ITarget target, Target databaseTarget) : base() {
            FilterCadence = new FilterCadenceFactory().Generate(project, target, databaseTarget);
        }

        public IExposure Select(DateTime atTime, IProject project, ITarget target, IExposure previousExposure) {
            if (AllExposurePlansRejected(target)) {
                throw new Exception($"unexpected: all exposure plans were rejected at exposure selection time for target '{target.Name}' at time {atTime}");
            }

            bool preDither = false;
            foreach (IFilterCadenceItem item in FilterCadence) {
                if (item.Action == FilterCadenceAction.Dither) {
                    preDither = true;
                    continue;
                }

                IExposure exposure = target.ExposurePlans[item.ReferenceIdx];
                if (!exposure.Rejected) {
                    exposure.PreDither = preDither;
                    FilterCadence.SetLastSelected(item);
                    return exposure;
                }
            }

            // Fail safe ... should not happen
            string msg = $"no acceptable exposure plan in override exposure selector for target '{target.Name}' at time {atTime}";
            TSLogger.Error(msg);
            throw new Exception(msg);
        }

        public void ExposureTaken(IExposure exposure) {
            FilterCadence.Advance();
        }

        public void TargetReset() {
        }
    }
}