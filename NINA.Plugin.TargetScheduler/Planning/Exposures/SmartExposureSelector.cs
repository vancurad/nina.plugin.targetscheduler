using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Select the next exposure automatically based on moon avoidance score.
    /// </summary>
    public class SmartExposureSelector : BaseExposureSelector, IExposureSelector {

        public SmartExposureSelector(IProject project, ITarget target, Target databaseTarget) : base(target) {
            DitherManager = GetDitherManager(project, target);
        }

        public IExposure Select(DateTime atTime, IProject project, ITarget target, IExposure previousExposure) {
            if (AllExposurePlansRejected(target)) {
                throw new Exception($"unexpected: all exposure plans were rejected at exposure selection time for target '{target.Name}' at time {atTime}");
            }

            // Find the accepted exposure with the highest score
            IExposure selected = null;
            double highScore = double.MinValue;

            LinkedList<IExposure> qualifyingExposures = new LinkedList<IExposure>();
            double leastProgressedExposure = Double.MaxValue;
            int leastProgressedExposureIndex = 0;

            foreach (IExposure exposure in target.ExposurePlans) {
                if (exposure.Rejected) continue;
                if (exposure.MoonAvoidanceScore >= highScore) {
                    double exposureProgress = (double)exposure.Accepted / (double)exposure.Desired;
                    if (qualifyingExposures.First != null && qualifyingExposures.First.Value.MoonAvoidanceScore == highScore) {
                        qualifyingExposures.AddLast(exposure);
                        if (exposureProgress < leastProgressedExposure) {
                            leastProgressedExposure = exposureProgress;
                            leastProgressedExposureIndex = qualifyingExposures.Count - 1;
                        }
                    } else {
                        qualifyingExposures = new LinkedList<IExposure>([exposure]);
                        leastProgressedExposure = exposureProgress;
                        leastProgressedExposureIndex = 0;
                    }
                    highScore = exposure.MoonAvoidanceScore;
                }
            }

            // If the previous exposure still qualifies and is >= 10% more progressed than the least progressed exposure, take the least progressed
            // This is to avoid erradic filter switching when all exposures have a mostly balanced progress.
            // If the previous exposure is no longer qualifying, take the exposure with the least amount of progress and highest moon avoidance score.
            double previousExposureProgress = previousExposure == null ? Double.MaxValue : (double)previousExposure.Accepted / (double)previousExposure.Desired;
            if (previousExposure != null && qualifyingExposures.Contains(previousExposure)) {
                if (previousExposureProgress - leastProgressedExposure > 0.1) {
                    selected = qualifyingExposures.ElementAt(leastProgressedExposureIndex);
                } else {
                    selected = previousExposure;
                }
            } else {
                selected = qualifyingExposures.ElementAt(leastProgressedExposureIndex);
            }

            if (selected == null) {
                // Fail safe ... should not happen
                string msg = $"unexpected: no acceptable exposure plan in smart exposure selector for target '{target.Name}' at time {atTime}";
                TSLogger.Error(msg);
                throw new Exception(msg);
            }

            selected.PreDither = DitherManager.DitherRequired(selected);
            return selected;
        }

        public void ExposureTaken(IExposure exposure) {
            if (exposure.PreDither) DitherManager.Reset();
            DitherManager.AddExposure(exposure);
        }

        public void TargetReset() {
            DitherManager.Reset();
        }
    }
}