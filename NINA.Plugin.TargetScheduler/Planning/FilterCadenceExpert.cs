using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class FilterCadenceExpert {
        private IProject project;
        private ITarget target;

        /* WIP - notes
*
* This is probably used by an over-arching exposure planner that handles auto-mode, moon avoidance, etc
*
* - Loads or lazy-creates the in-memory cadence item list (and attaches to Target)
* -
* - Probably not:
*   - Handles persistence as needed (including update of 'next' pointer)
*   - Handles clearing rows as needed
*
- Rows created during planning if not present - lazy load
- The rows for a target are cleared when:
- Any change to associated EPs
- Any change to Project FSF or dither frequency
- Any change to override exposure order
- If exposure planning goes to 'auto export' mode
- Target deletion
- Not copied when target is copied
*/

        public FilterCadenceExpert(IProject project, ITarget target) {
            this.project = project;
            this.target = target;
            target.FilterCadences = GenerateInitial();
        }

        public IFilterCadence GetNext() {
            foreach (IFilterCadence filterCadence in target.FilterCadences) {
                if (filterCadence.Next) return filterCadence;
            }

            return null;
        }

        public IExposure GetExposurePlanForFilterCadence(IFilterCadence filterCadence) {
            if (filterCadence is null) return null;
            if (filterCadence.Action == FilterCadenceAction.Dither) return null;
            return target.ExposurePlans[filterCadence.ReferenceIdx];
        }

        public List<IFilterCadence> GenerateInitial() {
            // Don't replace an existing list
            if (target.FilterCadences is not null && target.FilterCadences.Count > 0) {
                return target.FilterCadences;
            }

            List<IFilterCadence> filterCadences = new List<IFilterCadence>();

            // Generate from override list
            int order = 1;
            if (target.OverrideExposureOrders?.Count > 0) {
                target.OverrideExposureOrders.ForEach((oeo) => {
                    bool next = order == 1;
                    FilterCadenceAction action = oeo.Action == OverrideExposureOrderAction.Exposure ? FilterCadenceAction.Exposure : FilterCadenceAction.Dither;
                    filterCadences.Add(new PlanningFilterCadence(order++, next, action, oeo.ReferenceIdx));
                });

                return filterCadences;
            }

            // In these two cases, we don't have a fixed cadence known beforehand.  Instead,
            // we start empty and will add exposures and dithers as needed.
            int filterSwitchFrequency = project.FilterSwitchFrequency;
            if (filterSwitchFrequency == 0 || project.SmartExposureOrder) {
                return filterCadences;
            }

            // Generate from (non-zero) filter switch frequency
            int idx = 0;
            order = 1;
            target.ExposurePlans.ForEach((ep) => {
                for (int i = 0; i < filterSwitchFrequency; i++) {
                    bool next = order == 1;
                    filterCadences.Add(new PlanningFilterCadence(order++, next, FilterCadenceAction.Exposure, idx));
                }
                idx++;
            });

            return new DitherInjector(filterCadences, target.ExposurePlans, project.DitherEvery).Inject();
        }
    }
}