using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Select the next exposure based on the override exposure order for the target.
    /// </summary>
    public class OverrideOrderExposureSelector : BaseExposureSelector, IExposureSelector {

        public OverrideOrderExposureSelector() : base() {
        }

        public IExposure Select(DateTime atTime, IProject project, ITarget target) {
            // Since an override order generates an explicit filter cadence, we can just leverage the basic approach
            return new BasicExposureSelector().Select(atTime, project, target);
        }
    }
}