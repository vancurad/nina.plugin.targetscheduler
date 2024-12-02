using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;

namespace NINA.Plugin.TargetScheduler.Planning.Entities {

    public class PlanningOverrideExposureOrder : IOverrideExposureOrder {
        public int Order { get; set; }
        public OverrideExposureOrderAction Action { get; set; }
        public int ReferenceIdx { get; set; }

        public PlanningOverrideExposureOrder(OverrideExposureOrder oeo) {
            Order = oeo.Order;
            Action = oeo.Action;
            ReferenceIdx = oeo.ReferenceIdx;
        }

        public PlanningOverrideExposureOrder(int order, OverrideExposureOrderAction action, int referenceIdx) {
            Order = order;
            Action = action;
            ReferenceIdx = referenceIdx;
        }

        public override string ToString() {
            return $"order={Order}, action={Action}, refIdx={ReferenceIdx}";
        }
    }
}