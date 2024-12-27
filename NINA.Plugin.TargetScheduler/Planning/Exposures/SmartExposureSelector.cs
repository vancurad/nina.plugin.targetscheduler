using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Select the next exposure automatically based on moon avoidance score.
    /// </summary>
    public class SmartExposureSelector : BaseExposureSelector, IExposureSelector {

        public SmartExposureSelector(IProject project, ITarget target, Target databaseTarget) : base() {
            DitherManager = GetDitherManager(project, target);
        }

        public IExposure Select(DateTime atTime, IProject project, ITarget target, IExposure previousExposure) {
            if (AllExposurePlansRejected(target)) {
                throw new Exception($"unexpected: all exposure plans were rejected at exposure selection time for target '{target.Name}' at time {atTime}");
            }

            // Find the accepted exposure with the highest score
            IExposure selected = null;
            double highScore = double.MinValue;

            foreach (IExposure exposure in target.ExposurePlans) {
                if (exposure.Rejected) continue;
                if (exposure.MoonAvoidanceScore > highScore) {
                    selected = exposure;
                    highScore = exposure.MoonAvoidanceScore;
                }
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