using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class DitherInjector {
        private List<IFilterCadenceItem> filterCadences;
        private List<IExposure> exposuresPlans;
        private List<string> exposureOrder;
        private int ditherEvery;

        private List<string> uniqueFilters;

        public DitherInjector(List<IFilterCadenceItem> filterCadences, List<IExposure> exposuresPlans, int ditherEvery) {
            this.filterCadences = ShallowCopy(filterCadences);
            this.exposuresPlans = exposuresPlans;
            this.ditherEvery = ditherEvery;
        }

        public DitherInjector(List<string> exposureOrder, int ditherEvery) {
            this.exposureOrder = exposureOrder;
            this.ditherEvery = ditherEvery;
        }

        /// <summary>
        /// Insert fixed dithers using filter cadence list for filter switch frequency and dither every > 0.
        /// </summary>
        /// <returns></returns>
        public List<IFilterCadenceItem> Inject() {
            if (ditherEvery == 0) {
                return filterCadences;
            }

            if (filterCadences is null || filterCadences.Count == 0) {
                return filterCadences;
            }

            uniqueFilters = GetUniqueFilters();
            List<IFilterCadenceItem> dithered = new List<IFilterCadenceItem>();

            // Add first filter to the end to mimic a cycle and capture a final dither if needed
            filterCadences.Add(new PlanningFilterCadence(filterCadences[0], filterCadences[filterCadences.Count - 1].Order + 1, false));

            int pos = 0;
            int order = 1;
            while (pos < filterCadences.Count) {
                int ditherPos = FindNextDither(pos);
                if (ditherPos < 0) {
                    for (int i = pos; i < filterCadences.Count; i++) {
                        filterCadences[i].Order = order++;
                        dithered.Add(filterCadences[i]);
                    }

                    break;
                }

                for (int i = pos; i < ditherPos; i++) {
                    filterCadences[i].Order = order++;
                    dithered.Add(filterCadences[i]);
                }

                dithered.Add(new PlanningFilterCadence(order++, false, FilterCadenceAction.Dither, -1));
                pos = ditherPos;
            }

            // Remove duplicate first item from the end that we added above
            dithered.RemoveAt(dithered.Count - 1);

            return dithered;
        }

        /// <summary>
        /// Insert dithers for UI display.
        /// </summary>
        /// <returns></returns>
        public List<string> ExposureOrderInject() {
            if (ditherEvery == 0) {
                return exposureOrder;
            }

            if (exposureOrder is null || exposureOrder.Count == 0) {
                return exposureOrder;
            }

            // Add first filter to the end to mimic a cycle and capture a final dither if needed
            exposureOrder.Add(exposureOrder[0]);

            uniqueFilters = ExposureOrderGetUniqueFilters();
            List<string> dithered = new List<string>();

            int pos = 0;
            while (pos < exposureOrder.Count) {
                int ditherPos = ExposureOrderFindNextDither(pos);
                if (ditherPos < 0) {
                    for (int i = pos; i < exposureOrder.Count; i++) {
                        dithered.Add(exposureOrder[i]);
                    }

                    break;
                }

                for (int i = pos; i < ditherPos; i++) {
                    dithered.Add(exposureOrder[i]);
                }

                dithered.Add(OverrideExposureOrderAction.Dither.ToString());
                pos = ditherPos;
            }

            // Remove duplicate first item from the end that we added above
            dithered.RemoveAt(dithered.Count - 1);

            return dithered;
        }

        private List<IFilterCadenceItem> ShallowCopy(List<IFilterCadenceItem> filterCadences) {
            return filterCadences is null
                ? new List<IFilterCadenceItem>()
                : new List<IFilterCadenceItem>(filterCadences);
        }

        private int FindNextDither(int start) {
            Dictionary<string, int> filterCounts = GetFilterDictionary();

            // Walk the list, incrementing when each filter occurs.  Injection point is when a filter is seen ditherEvery+1 times.
            int pos = -1;
            for (int i = start; i < filterCadences.Count; i++) {
                if (filterCadences[i].Action is FilterCadenceAction.Exposure) {
                    string filterName = exposuresPlans[filterCadences[i].ReferenceIdx].FilterName;
                    filterCounts[filterName]++;
                    if (filterCounts[filterName] == ditherEvery + 1) {
                        pos = i;
                        break;
                    }
                }
            }

            return pos;
        }

        private int ExposureOrderFindNextDither(int start) {
            Dictionary<string, int> filterCounts = GetFilterDictionary();

            // Walk the list, incrementing when each filter occurs.  Injection point is when a filter is seen ditherEvery+1 times.
            int pos = -1;
            for (int i = start; i < exposureOrder.Count; i++) {
                string filterName = (exposureOrder[i]);
                filterCounts[filterName]++;
                if (filterCounts[filterName] == ditherEvery + 1) {
                    pos = i;
                    break;
                }
            }

            return pos;
        }

        private List<string> GetUniqueFilters() {
            List<string> filters = new List<string>();
            foreach (IFilterCadenceItem fc in filterCadences) {
                if (fc.Action is FilterCadenceAction.Exposure) {
                    string filterName = exposuresPlans[fc.ReferenceIdx].FilterName;
                    if (!filters.Contains(filterName)) {
                        filters.Add(filterName);
                    }
                }
            }

            return filters;
        }

        private List<string> ExposureOrderGetUniqueFilters() {
            List<string> filters = new List<string>();
            foreach (string exposure in exposureOrder) {
                if (!filters.Contains(exposure)) {
                    filters.Add(exposure);
                }
            }

            return filters;
        }

        private Dictionary<string, int> GetFilterDictionary() {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (string filter in uniqueFilters) {
                dict.Add(filter, 0);
            }

            return dict;
        }
    }
}