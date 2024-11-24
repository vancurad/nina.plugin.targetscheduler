using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Database.Schema {

    public enum FilterCadenceAction {
        Exposure = 0, Dither = 1,
    }

    public class FilterCadence : INotifyPropertyChanged {
        [Key] public int Id { get; set; }

        [Required] public int TargetId { get; set; }
        [Required] public int order { get; set; }
        public bool next { get; set; }
        [Required] public int action { get; set; }
        public int referenceIdx { get; set; }

        public FilterCadence() {
        }

        public FilterCadence(int targetId, int order, bool next, int action, int referenceIdx) {
            this.TargetId = targetId;
            this.order = order;
            this.next = next;
            this.action = action;
            this.referenceIdx = referenceIdx;
        }

        public FilterCadence(int targetId, int order, bool next, int action)
            : this(targetId, order, next, action, -1) { }

        public FilterCadence(int targetId, int order, bool next, FilterCadenceAction action, int referenceIdx)
            : this(targetId, order, next, (int)action, referenceIdx) { }

        public FilterCadence(int targetId, int order, bool next, FilterCadenceAction action)
            : this(targetId, order, next, (int)action, -1) { }

        [NotMapped]
        public int Order {
            get => order;
            set {
                order = value;
                RaisePropertyChanged(nameof(Order));
            }
        }

        [NotMapped]
        public bool Next {
            get { return next; }
            set {
                next = value;
                RaisePropertyChanged(nameof(Next));
            }
        }

        [NotMapped]
        public FilterCadenceAction Action {
            get { return (FilterCadenceAction)action; }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"TargetId: {TargetId}");
            sb.AppendLine($"Order: {Order}");
            sb.AppendLine($"Next: {Next}");
            sb.AppendLine($"Action: {Action}");
            sb.AppendLine($"RefIdx: {ReferenceIdx}");

            return sb.ToString();
        }
    }
}