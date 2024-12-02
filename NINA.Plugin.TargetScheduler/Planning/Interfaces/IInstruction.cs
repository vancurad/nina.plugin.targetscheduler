namespace NINA.Plugin.TargetScheduler.Planning.Interfaces {

    public interface IInstruction {
        IExposure planExposure { get; set; }
    }
}