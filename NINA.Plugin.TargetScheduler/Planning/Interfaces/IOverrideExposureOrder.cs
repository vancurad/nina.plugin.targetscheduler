using NINA.Plugin.TargetScheduler.Database.Schema;

namespace NINA.Plugin.TargetScheduler.Planning.Interfaces {

    public interface IOverrideExposureOrder {
        int Order { get; }
        OverrideExposureOrderAction Action { get; }
        int ReferenceIdx { get; }
    }
}