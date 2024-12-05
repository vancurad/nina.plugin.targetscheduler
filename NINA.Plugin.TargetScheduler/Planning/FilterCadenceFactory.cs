using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class FilterCadenceFactory {

        public FilterCadenceFactory() {
        }

        public FilterCadence Generate(IProject project, ITarget planTarget, Target databaseTarget) {
            List<IFilterCadenceItem> filterCadences = new List<IFilterCadenceItem>();

            // Use database records if present
            if (Common.IsNotEmpty(databaseTarget.FilterCadences)) {
                databaseTarget.FilterCadences.ForEach(fc => { filterCadences.Add(new PlanningFilterCadence(fc)); });
                return new FilterCadence(filterCadences);
            }

            // Generate from override list
            int order = 1;
            if (Common.IsNotEmpty(planTarget.OverrideExposureOrders)) {
                planTarget.OverrideExposureOrders.ForEach((oeo) => {
                    bool next = order == 1;
                    FilterCadenceAction action = oeo.Action == OverrideExposureOrderAction.Exposure ? FilterCadenceAction.Exposure : FilterCadenceAction.Dither;
                    filterCadences.Add(new PlanningFilterCadence(order++, next, action, oeo.ReferenceIdx));
                });

                return new FilterCadence(filterCadences);
            }

            // In these two cases, we don't have a fixed cadence known beforehand.  Instead,
            // we start empty and will add exposures and dithers later as needed.
            int filterSwitchFrequency = project.FilterSwitchFrequency;
            if (filterSwitchFrequency == 0 || project.SmartExposureOrder) {
                return new FilterCadence(filterCadences);
            }

            // Otherwise, generate from (non-zero) filter switch frequency
            int idx = 0;
            order = 1;
            planTarget.ExposurePlans.ForEach((ep) => {
                for (int i = 0; i < filterSwitchFrequency; i++) {
                    bool next = order == 1;
                    filterCadences.Add(new PlanningFilterCadence(order++, next, FilterCadenceAction.Exposure, idx));
                }
                idx++;
            });

            return new FilterCadence(new DitherInjector(filterCadences, planTarget.ExposurePlans, project.DitherEvery).Inject());
        }
    }
}