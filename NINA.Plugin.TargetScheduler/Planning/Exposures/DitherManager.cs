using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

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
            int count = exposureStack.Count(item => item.FilterName == nextExposure.FilterName);
            return count >= ditherEvery;
        }

        public void Reset() {
            exposureStack.Clear();
        }
    }

    public class DitherManagerCache {
        private static readonly TimeSpan ITEM_TIMEOUT = TimeSpan.FromHours(12);
        private static MemoryCache _cache = CreateCache();

        public static DitherManager Get(string cacheKey) {
            return (DitherManager)_cache.Get(cacheKey);
        }

        public static void Put(DitherManager ditherManager, string cacheKey) {
            _cache.Add(cacheKey, ditherManager, DateTime.Now.Add(ITEM_TIMEOUT));
        }

        public static void Clear() {
            _cache.Dispose();
            _cache = CreateCache();
        }

        private static MemoryCache CreateCache() {
            return new MemoryCache("Scheduler DitherManager");
        }

        private DitherManagerCache() {
        }
    }
}