using NINA.Plugin.TargetScheduler.Planning.Interfaces;

namespace NINA.Plugin.TargetScheduler.Planning.Scoring.Rules {

    public class SmartExposureOrderRule : ScoringRule {
        public const string RULE_NAME = "Smart Exposure Order";
        public const double DEFAULT_WEIGHT = 0 * WEIGHT_SCALE;

        public override string Name { get { return RULE_NAME; } }
        public override double DefaultWeight { get { return DEFAULT_WEIGHT; } }

        /// <summary>
        /// Score the potential target based on whether the project is using smart exposure order
        /// and the selected exposure's score.
        ///
        /// Scores are calculated by the MoonAvoidanceExpert and normalized to 0-1.  Note that
        /// score calculation assumes that usage (like sorting by score) is only for comparison
        /// against scores for the same target.  However, here it can be taken as a gross metric
        /// for higher moon aversion criteria.
        /// </summary>
        /// <param name="scoringEngine"></param>
        /// <param name="potentialTarget"></param>
        /// <returns></returns>
        public override double Score(IScoringEngine scoringEngine, ITarget potentialTarget) {
            IProject project = potentialTarget.Project;
            return project.SmartExposureOrder
                ? potentialTarget.SelectedExposure.MoonAvoidanceScore
                : 0;
        }
    }
}