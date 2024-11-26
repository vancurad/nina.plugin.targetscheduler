using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Database.Schema {

    public enum OverrideExposureOrderAction {
        Exposure = 0, Dither = 1,
    }

    public class OverrideExposureOrder : INotifyPropertyChanged {
        [Key] public int Id { get; set; }

        [Required] public int TargetId { get; set; }
        [Required] public int order { get; set; }
        [Required] public int action { get; set; }
        public int referenceIdx { get; set; }

        public OverrideExposureOrder() {
        }

        public OverrideExposureOrder(int targetId, int order, int action, int referenceIdx) {
            this.TargetId = targetId;
            this.order = order;
            this.action = action;
            this.referenceIdx = referenceIdx;
        }

        public OverrideExposureOrder(int targetId, int order, int action)
            : this(targetId, order, action, -1) { }

        public OverrideExposureOrder(int targetId, int order, OverrideExposureOrderAction action, int referenceIdx)
            : this(targetId, order, (int)action, referenceIdx) { }

        public OverrideExposureOrder(int targetId, int order, OverrideExposureOrderAction action)
            : this(targetId, order, (int)action, -1) { }

        [NotMapped]
        public int Order {
            get => order;
            set {
                order = value;
                RaisePropertyChanged(nameof(Order));
            }
        }

        [NotMapped]
        public OverrideExposureOrderAction Action {
            get { return (OverrideExposureOrderAction)action; }
            set {
                action = (int)value;
                RaisePropertyChanged(nameof(Action));
            }
        }

        [NotMapped]
        public int ReferenceIdx {
            get => referenceIdx;
            set {
                referenceIdx = value;
                RaisePropertyChanged(nameof(ReferenceIdx));
            }
        }

        public OverrideExposureOrder GetPasteCopy(int targetId) {
            OverrideExposureOrder copy = new OverrideExposureOrder();
            copy.TargetId = targetId;
            copy.order = order;
            copy.action = action;
            copy.referenceIdx = referenceIdx;
            return copy;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"TargetId: {TargetId}");
            sb.AppendLine($"Order: {Order}");
            sb.AppendLine($"Action: {Action}");
            sb.AppendLine($"RefIdx: {ReferenceIdx}");

            return sb.ToString();
        }

        public override bool Equals(object obj) {
            if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
                return false;
            }

            OverrideExposureOrder other = obj as OverrideExposureOrder;
            return TargetId == other.TargetId &&
                Order == other.Order &&
                Action == other.Action &&
                ReferenceIdx == other.ReferenceIdx;
        }
    }
}