using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Select the next exposure automatically, primarily based on moon avoidance.
    /// </summary>
    public class SmartExposureSelector : BaseExposureSelector, IExposureSelector {

        public SmartExposureSelector(DateTime atTime) : base(atTime) {
        }

        public IExposure Select(IProject project, ITarget target) {
            return null;
        }
    }
}