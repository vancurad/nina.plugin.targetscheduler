using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;

namespace NINA.Plugin.TargetScheduler.Planning.Exposures {

    /// <summary>
    /// Select the next exposure automatically, primarily based on moon avoidance.
    /// </summary>
    public class SmartExposureSelector : BaseExposureSelector, IExposureSelector {

        public SmartExposureSelector(IProject project, ITarget target, Target databaseTarget) : base() {
        }

        public IExposure Select(DateTime atTime, IProject project, ITarget target, IExposure previousExposure) {
            throw new NotImplementedException();
        }

        public void ExposureTaken(IExposure exposure) {
            throw new NotImplementedException();
        }

        public void TargetReset() {
            throw new NotImplementedException();
        }
    }
}