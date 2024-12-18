using NINA.Plugin.TargetScheduler.Planning.Interfaces;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    public abstract class BaseExposureSelector {
        protected FilterCadence FilterCadence;
        protected DitherManager DitherManager;

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
    }
}