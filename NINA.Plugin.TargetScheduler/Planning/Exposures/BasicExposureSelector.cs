using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Select the next exposure based on the persisted filter cadence for this target.
    /// </summary>
    public class BasicExposureSelector : BaseExposureSelector, IExposureSelector {

        public BasicExposureSelector(DateTime atTime) : base(atTime) {
        }

        public IExposure Select(IProject project, ITarget target) {
            //
            /* TODO:
             * - For <GetNext> in cadence list:
             *   - if Dither: PreDither = true
             *   - if Exposure && associated EP is not rejected (also for twilight level!): this is our exposure, return
             *
             */

            // Exp Plans: L,R,G,B
            // FC LRGBd, next is R

            FilterCadenceExpert expert = new FilterCadenceExpert(project, target);

            int nextIdx = target.FilterCadences.FindIndex(fc => fc.Next);

            // TODO: does it make sense to formulate the FC list as a circular list?
            // just treat the list as a circle: https://stackoverflow.com/questions/33781853/circular-lists-in-c-sharp

            throw new NotImplementedException();
        }
    }
}