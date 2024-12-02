using NINA.Plugin.TargetScheduler.Database.Schema;

namespace NINA.Plugin.TargetScheduler.Planning.Interfaces {

    public interface IFilterCadence {
        int Order { get; }
        bool Next { get; }
        FilterCadenceAction Action { get; }
        int ReferenceIdx { get; }
    }
}