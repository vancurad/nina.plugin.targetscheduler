using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
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

            FilterCadence filterCadence = target.FilterCadence;
            bool preDither = false;
            int items = 0;

            foreach (var item in filterCadence) {
            }

            // TODO: also, if we end up selecting one that wasn't Next=true, we need to account for that
            // when finally doing the Advance()

            // OLD
            while (true) {
                IFilterCadenceItem item = filterCadence.GetNext();
                if (item.Action == Database.Schema.FilterCadenceAction.Dither) {
                    preDither = true;
                    continue;
                }

                // Be sure we haven't been all the way around without finding an exposure
                if (++items == filterCadence.Count) {
                    TSLogger.Warning("looped around on filter cadence list witout finding a suitable exposure");
                    return null;
                }

                // REALLY?  We don't really want to advance until we've taken the exposure!!
                // Instead, we want an
                //filterCadence.Advance();
            }

            throw new NotImplementedException();
        }
    }
}