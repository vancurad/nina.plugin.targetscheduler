using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Planning.Scoring.Rules;
using NINA.Plugin.TargetScheduler.Test.Planning;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Plan.Scoring.Rules {

    [TestFixture]
    public class MeridianWindowPriorityRuleTest {

        [Test]
        public void testMeridianWindowPriority() {
            Mock<IScoringEngine> scoringEngineMock = PlanMocks.GetMockScoringEnging();
            Mock<ITarget> targetMock = new Mock<ITarget>().SetupAllProperties();
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("pp", ProjectState.Active);
            targetMock.SetupProperty(m => m.Project, pp.Object);

            MeridianWindowPriorityRule sut = new MeridianWindowPriorityRule();

            pp.SetupProperty(m => m.MeridianWindow, 0);
            sut.Score(scoringEngineMock.Object, targetMock.Object).Should().BeApproximately(0, 0.00001);

            pp.SetupProperty(m => m.MeridianWindow, 60);
            sut.Score(scoringEngineMock.Object, targetMock.Object).Should().BeApproximately(1, 0.00001);
        }
    }
}