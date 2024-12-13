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

        public IExposureSelector GetExposureSelector(IProject project, ITarget target) {
            if (project.SmartExposureOrder) {
                return new SmartExposureSelector();
            }

            if (target.OverrideExposureOrders.Count > 0) {
                return new OverrideOrderExposureSelector();
            }

            return new BasicExposureSelector();
        }
    }

    public interface IExposureSelector {

        IExposure Select(DateTime atTime, IProject project, ITarget target, IExposure previousExposure);
    }
}