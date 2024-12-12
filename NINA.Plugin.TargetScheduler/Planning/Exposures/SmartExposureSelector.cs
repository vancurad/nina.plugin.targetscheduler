using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Select the next exposure automatically, primarily based on moon avoidance.
    /// </summary>
    public class SmartExposureSelector : BaseExposureSelector, IExposureSelector {

        public SmartExposureSelector() : base() {
        }

        public IExposure Select(DateTime atTime, IProject project, ITarget target) {
            throw new NotImplementedException("SmartExposureSelector.Select not implement");
        }
    }
}