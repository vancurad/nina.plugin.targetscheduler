namespace NINA.Plugin.TargetScheduler.Planning.Interfaces {

    public interface IScoringRule {
        string Name { get; }
        double DefaultWeight { get; }

        double Score(IScoringEngine scoringEngine, ITarget potentialTarget);
    }
}