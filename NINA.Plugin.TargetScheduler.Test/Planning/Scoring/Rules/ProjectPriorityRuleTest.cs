using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Planning.Scoring.Rules;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Scoring.Rules {

    [TestFixture]
    public class ProjectPriorityRuleTest {

        [Test]
        public void testProjectPriority() {
            Mock<IScoringEngine> scoringEngineMock = PlanMocks.GetMockScoringEnging();
            Mock<ITarget> targetMock = new Mock<ITarget>().SetupAllProperties();
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("pp", ProjectState.Active);
            targetMock.SetupProperty(m => m.Project, pp.Object);

            ProjectPriorityRule sut = new ProjectPriorityRule();

            pp.SetupProperty(m => m.Priority, ProjectPriority.Low);
            sut.Score(scoringEngineMock.Object, targetMock.Object).Should().BeApproximately(0, 0.00001);

            pp.SetupProperty(m => m.Priority, ProjectPriority.Normal);
            sut.Score(scoringEngineMock.Object, targetMock.Object).Should().BeApproximately(0.5, 0.00001);

            pp.SetupProperty(m => m.Priority, ProjectPriority.High);
            sut.Score(scoringEngineMock.Object, targetMock.Object).Should().BeApproximately(1, 0.00001);
        }
    }
}