namespace NINA.Plugin.TargetScheduler.Planning.Interfaces {

    public interface IPlanningInstruction {
        IExposure planExposure { get; set; }
    }
}