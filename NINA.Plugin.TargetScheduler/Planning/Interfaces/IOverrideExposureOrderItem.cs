using NINA.Plugin.TargetScheduler.Database.Schema;

namespace NINA.Plugin.TargetScheduler.Planning.Interfaces {

    public interface IOverrideExposureOrderItem {
        int Order { get; }
        OverrideExposureOrderAction Action { get; }
        int ReferenceIdx { get; }
    }
}