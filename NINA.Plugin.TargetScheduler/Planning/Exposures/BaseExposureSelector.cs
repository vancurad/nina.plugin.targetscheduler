using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    public abstract class BaseExposureSelector {

        public BaseExposureSelector() {
        }

        /// <summary>
        /// Return true if all exposure plans were rejected, otherwise false.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool AllExposurePlansRejected(ITarget target) {
            bool atLeastOneAccepted = false;
            target.ExposurePlans.ForEach(e => { if (!e.Rejected) atLeastOneAccepted = true; });
            return !atLeastOneAccepted;
        }

        /// <summary>
        /// Determine whether a requested dither should be skipped or not.
        /// </summary>
        /// <param name="enabled"></param>
        /// <param name="exposureSkipped"></param>
        /// <param name="exposure"></param>
        /// <param name="previousExposure"></param>
        /// <returns></returns>
        public bool DitherRequired(bool enabled, bool exposureSkipped, IExposure exposure, IExposure previousExposure) {
            if (!enabled) { return false; }
            if (!exposureSkipped) { return true; }
            return exposure != previousExposure;
        }

        public bool AddDither(IExposure exposure, List<IExposure> targetExposureHistory) {
            // Never a need to dither if starting since a slew should have just been done
            if (targetExposureHistory.Count == 0) { return false; }

            // How many of exposure.filter have we seen most recently?
            // OMG is the history really a stack?  And cleared on a dither?

            return false;
        }
    }
}