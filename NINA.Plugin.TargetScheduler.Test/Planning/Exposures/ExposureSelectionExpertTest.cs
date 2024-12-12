using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Exposures;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Test.Astrometry;
using NUnit.Framework;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Exposures {

    [TestFixture]
    public class ExposureSelectionExpertTest {

        [Test]
        public void testBasicExposureSelector() {
            Mock<IProject> mockProject = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            mockProject.SetupAllProperties();
            mockProject.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            mockTarget.SetupAllProperties();
            mockTarget.SetupProperty(t => t.OverrideExposureOrders, new List<IOverrideExposureOrderItem>());

            IExposureSelector s = new ExposureSelectionExpert().GetExposureSelector(mockProject.Object, mockTarget.Object);
            (s is BasicExposureSelector).Should().BeTrue();
        }

        [Test]
        public void testSmartExposureSelector() {
            Mock<IProject> mockProject = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            mockProject.SetupAllProperties();
            mockProject.SetupProperty(p => p.SmartExposureOrder, true);
            Mock<ITarget> mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            mockTarget.SetupAllProperties();

            IExposureSelector s = new ExposureSelectionExpert().GetExposureSelector(mockProject.Object, mockTarget.Object);
            (s is SmartExposureSelector).Should().BeTrue();
        }

        [Test]
        public void testOverrideOrderExposureSelector() {
            Mock<IProject> mockProject = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            mockProject.SetupAllProperties();
            mockProject.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            mockTarget.SetupAllProperties();
            Mock<IOverrideExposureOrderItem> oeo = new Mock<IOverrideExposureOrderItem>();
            List<IOverrideExposureOrderItem> oeos = new List<IOverrideExposureOrderItem>() { oeo.Object };
            mockTarget.SetupProperty(t => t.OverrideExposureOrders, oeos);

            IExposureSelector s = new ExposureSelectionExpert().GetExposureSelector(mockProject.Object, mockTarget.Object);
            (s is OverrideOrderExposureSelector).Should().BeTrue();
        }
    }
}