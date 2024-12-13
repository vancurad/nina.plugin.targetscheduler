using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Manage the dither state for a target.  We maintain a stack of exposures since the last
    /// dither.  When a new exposure is selected, we need to dither if the count of that
    /// exposure filter in the stack is equal to the 'dither every' value.  The stack should be
    /// reset (cleared) after a successful dither/exposure.
    ///
    /// Note that the comparison is on the filter name - not the exposure plan itself.  Since you
    /// could have different exposure plans using the same filter, we want to ensure that when that
    /// filter is selected again, we dither appropriately.
    ///
    /// Also, we don't reset the stack here if a dither is required since we don't know at that
    /// point whether the exposure will actually be taken and pre-dither performed.
    /// </summary>
    public class DitherManager {
        private Stack<IExposure> exposureStack;
        private int ditherEvery;

        public DitherManager(int ditherEvery) {
            this.ditherEvery = ditherEvery;
            exposureStack = new Stack<IExposure>();
        }

        public void AddExposure(IExposure exposure) {
            exposureStack.Push(exposure);
        }

        public bool DitherRequired(IExposure nextExposure) {
            if (ditherEvery == 0) { return false; }

            int count = 0;
            foreach (var item in exposureStack) {
                if (item.FilterName == nextExposure.FilterName) { count++; }
            }

            return count >= ditherEvery;
        }

        public void Reset() {
            exposureStack.Clear();
        }
    }
}