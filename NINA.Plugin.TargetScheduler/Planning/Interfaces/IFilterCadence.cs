using NINA.Plugin.TargetScheduler.Database.Schema;

namespace NINA.Plugin.TargetScheduler.Planning.Interfaces {

    public interface IFilterCadence {
        int Order { get; set; }
        bool Next { get; set; }
        FilterCadenceAction Action { get; set; }
        int ReferenceIdx { get; set; }
    }
}