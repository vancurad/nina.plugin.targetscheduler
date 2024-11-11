using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Planning.Scoring.Rules;
using NINA.Plugin.TargetScheduler.Test.Astrometry;
using NINA.Plugin.TargetScheduler.Test.Planning;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Plan.Scoring.Rules {

    [TestFixture]
    public class TargetSwitchPenaltyRuleTest {

        [Test]
        public void testTargetSwitchPenalty1() {
            Mock<IScoringEngine> scoringEngineMock = PlanMocks.GetMockScoringEnging();
            Mock<ITarget> targetMock = PlanMocks.GetMockPlanTarget("", TestData.SPICA);
            targetMock.Setup(m => m.Equals(It.IsAny<object>())).Returns(true);

            TargetSwitchPenaltyRule sut = new TargetSwitchPenaltyRule();
            sut.Score(scoringEngineMock.Object, targetMock.Object).Should().BeApproximately(1, 0.00001);
        }

        [Test]
        public void testTargetSwitchPenalty0() {
            Mock<IScoringEngine> scoringEngineMock = PlanMocks.GetMockScoringEnging();
            Mock<ITarget> targetMock = PlanMocks.GetMockPlanTarget("", TestData.SPICA);
            targetMock.Setup(m => m.Equals(It.IsAny<object>())).Returns(false);

            TargetSwitchPenaltyRule sut = new TargetSwitchPenaltyRule();
            sut.Score(scoringEngineMock.Object, targetMock.Object).Should().BeApproximately(0, 0.00001);
        }
    }
}