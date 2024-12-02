using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Determine the appropriate exposure selector approach based on project/target settings and
    /// select and next best exposure.
    /// </summary>
    public class ExposureSelectionExpert {

        public ExposureSelectionExpert() {
        }

        /* TODO:
         * - Test for BasicExposureSelector
         * - Implement SmartExposureOrder and test
         * - Implement SmartExposureOrder and test
         * - Test for ExposureSelectionExpert
         *
         */

        public IExposure Select(DateTime atTime, IProject project, ITarget target) {
            if (project.SmartExposureOrder) {
                return new SmartExposureSelector(atTime).Select(project, target);
            }

            if (target.OverrideExposureOrders.Count > 0) {
                return new OverrideOrderExposureSelector(atTime).Select(project, target);
            }

            return new BasicExposureSelector(atTime).Select(project, target);
        }
    }

    public interface IExposureSelector {

        IExposure Select(IProject project, ITarget target);
    }
}