﻿using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class FilterCadenceFactory {
        private object lockObj = new object();

        public FilterCadenceFactory() {
        }

        public FilterCadence Generate(IProject project, ITarget target) {
            if (target.FilterCadence is not null) return target.FilterCadence;

            List<IFilterCadenceItem> filterCadences = new List<IFilterCadenceItem>();

            // Generate from override list
            int order = 1;
            if (target.OverrideExposureOrders?.Count > 0) {
                target.OverrideExposureOrders.ForEach((oeo) => {
                    bool next = order == 1;
                    FilterCadenceAction action = oeo.Action == OverrideExposureOrderAction.Exposure ? FilterCadenceAction.Exposure : FilterCadenceAction.Dither;
                    filterCadences.Add(new PlanningFilterCadence(order++, next, action, oeo.ReferenceIdx));
                });

                return new FilterCadence(filterCadences);
            }

            // In these two cases, we don't have a fixed cadence known beforehand.  Instead,
            // we start empty and will add exposures and dithers as needed.
            int filterSwitchFrequency = project.FilterSwitchFrequency;
            if (filterSwitchFrequency == 0 || project.SmartExposureOrder) {
                return new FilterCadence(filterCadences);
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

            return new FilterCadence(new DitherInjector(filterCadences, target.ExposurePlans, project.DitherEvery).Inject());
        }
    }
}