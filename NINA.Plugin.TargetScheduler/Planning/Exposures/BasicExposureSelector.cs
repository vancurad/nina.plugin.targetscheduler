using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Select the next exposure based on the project's FilterSwitchFrequency and the persisted FilterCadence
    /// for this target.
    /// </summary>
    public class BasicExposureSelector : BaseExposureSelector, IExposureSelector {

        public BasicExposureSelector(DateTime atTime) : base(atTime) {
        }

        public IExposure Select(IProject project, ITarget target) {
            /* TODO:
             * - for each exp that is not rejected and is OK for the current level of twilight
             */

            // Exp Plans: L,R,G,B
            // FC LRGBd, next is R

            FilterCadenceExpert expert = new FilterCadenceExpert(project, target);

            //target.FilterCadences
            //project.FilterSwitchFrequency
            // note that FilterCadenceExpert exists but is unimplemented
            //target.Fil

            throw new NotImplementedException();
        }
    }
}