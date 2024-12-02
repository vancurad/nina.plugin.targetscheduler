using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    public abstract class BaseExposureSelector {
        public DateTime AtTime { get; private set; }

        public BaseExposureSelector(DateTime atTime) {
            this.AtTime = atTime;
        }
    }
}