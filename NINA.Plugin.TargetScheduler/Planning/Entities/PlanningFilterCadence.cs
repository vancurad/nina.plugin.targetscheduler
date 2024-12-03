using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;

namespace NINA.Plugin.TargetScheduler.Planning.Entities {

    public class PlanningFilterCadence : IFilterCadence {
        public int Order { get; set; }
        public bool Next { get; set; }
        public FilterCadenceAction Action { get; set; }
        public int ReferenceIdx { get; set; }

        public PlanningFilterCadence(FilterCadence filterCadence) {
            Order = filterCadence.Order;
            Next = filterCadence.Next;
            Action = filterCadence.Action;
            ReferenceIdx = filterCadence.ReferenceIdx;
        }

        public PlanningFilterCadence(int order, bool next, FilterCadenceAction action, int referenceIdx) {
            Order = order;
            Next = next;
            Action = action;
            ReferenceIdx = referenceIdx;
        }

        public PlanningFilterCadence(IFilterCadence filterCadence, int order, bool next) {
            Order = order;
            Next = next;
            Action = filterCadence.Action;
            ReferenceIdx = filterCadence.ReferenceIdx;
        }

        public override string ToString() {
            return $"order={Order}, next={Next}, action={Action}, refIdx={ReferenceIdx}";
        }
    }
}