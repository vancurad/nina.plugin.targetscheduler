using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Planning.Scoring.Rules;
using NINA.Plugin.TargetScheduler.Test.Astrometry;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Scoring.Rules {

    [TestFixture]
    public class SmartExposureOrderRuleTest {

        [Test]
        public void testNotSmart() {
            Mock<IScoringEngine> scoringEngineMock = PlanMocks.GetMockScoringEnging();
            Mock<IProject> projectMock = PlanMocks.GetMockPlanProject("p1", ProjectState.Active);
            projectMock.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> targetMock = PlanMocks.GetMockPlanTarget("", TestData.SPICA);
            targetMock.SetupProperty(m => m.Project, projectMock.Object);

            Mock<IExposure> exposurePlanMock = PlanMocks.GetMockPlanExposure("", 10, 0);
            PlanMocks.AddMockPlanFilter(targetMock, exposurePlanMock);

            SmartExposureOrderRule sut = new SmartExposureOrderRule();
            sut.Score(scoringEngineMock.Object, targetMock.Object).Should().BeApproximately(0, 0.00001);
        }

        [Test]
        public void testSmart() {
            Mock<IScoringEngine> scoringEngineMock = PlanMocks.GetMockScoringEnging();
            Mock<IProject> projectMock = PlanMocks.GetMockPlanProject("p1", ProjectState.Active);
            projectMock.SetupProperty(p => p.SmartExposureOrder, true);

            Mock<IExposure> exposurePlanMock = PlanMocks.GetMockPlanExposure("", 10, 0);
            exposurePlanMock.SetupProperty(m => m.MoonAvoidanceScore, .5);

            Mock<ITarget> targetMock = PlanMocks.GetMockPlanTarget("", TestData.SPICA);
            targetMock.SetupProperty(m => m.Project, projectMock.Object);
            targetMock.SetupProperty(m => m.SelectedExposure, exposurePlanMock.Object);
            PlanMocks.AddMockPlanFilter(targetMock, exposurePlanMock);

            SmartExposureOrderRule sut = new SmartExposureOrderRule();
            sut.Score(scoringEngineMock.Object, targetMock.Object).Should().BeApproximately(0.5, 0.00001);
        }
    }
}