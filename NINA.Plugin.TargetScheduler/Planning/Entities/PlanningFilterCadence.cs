using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;

namespace NINA.Plugin.TargetScheduler.Planning.Entities {

    public class PlanningFilterCadence : IFilterCadenceItem {
        public int Order { get; set; }
        public bool Next { get; set; }
        public FilterCadenceAction Action { get; set; }
        public int ReferenceIdx { get; set; }

        public PlanningFilterCadence(FilterCadenceItem filterCadence) {
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

        public PlanningFilterCadence(IFilterCadenceItem filterCadence, int order, bool next) {
            Order = order;
            Next = next;
            Action = filterCadence.Action;
            ReferenceIdx = filterCadence.ReferenceIdx;
        }

        public override string ToString() {
            return $"order={Order}, next={Next}, action={Action}, refIdx={ReferenceIdx}";
        }

        public override bool Equals(object obj) {
            if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
                return false;
            }

            PlanningFilterCadence other = obj as PlanningFilterCadence;
            return Order == other.Order &&
                Next == other.Next &&
                Action == other.Action &&
                ReferenceIdx == other.ReferenceIdx;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}