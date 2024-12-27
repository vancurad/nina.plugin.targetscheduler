using NINA.Plugin.TargetScheduler.Planning.Interfaces;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    public abstract class BaseExposureSelector {
        protected FilterCadence FilterCadence;
        protected DitherManager DitherManager;

        public BaseExposureSelector() {
        }

        /// <summary>
        /// Some exposure selectors need to remember the previous dither state - typically those
        /// that don't rely on a persisted FilterCadence.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public DitherManager GetDitherManager(IProject project, ITarget target) {
            string cacheKey = $"{target.DatabaseId}";
            DitherManager dm = DitherManagerCache.Get(cacheKey);
            if (dm != null) {
                return dm;
            } else {
                dm = new DitherManager(project.DitherEvery);
                DitherManagerCache.Put(dm, cacheKey);
                return dm;
            }
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