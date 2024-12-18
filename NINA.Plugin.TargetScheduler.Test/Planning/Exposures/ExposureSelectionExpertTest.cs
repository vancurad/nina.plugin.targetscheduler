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
            mockProject.SetupProperty(p => p.FilterSwitchFrequency, 1);
            mockProject.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            mockTarget.SetupAllProperties();
            mockTarget.SetupProperty(t => t.ExposurePlans, new List<IExposure>());

            IExposureSelector s = new ExposureSelectionExpert().GetExposureSelector(mockProject.Object, mockTarget.Object, new Target());
            (s is BasicExposureSelector).Should().BeTrue();
        }

        [Test]
        public void testSmartExposureSelector() {
            Mock<IProject> mockProject = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            mockProject.SetupAllProperties();
            mockProject.SetupProperty(p => p.SmartExposureOrder, true);
            Mock<ITarget> mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            mockTarget.SetupAllProperties();

            IExposureSelector s = new ExposureSelectionExpert().GetExposureSelector(mockProject.Object, mockTarget.Object, new Target());
            (s is SmartExposureSelector).Should().BeTrue();
        }

        [Test]
        public void testRepeatUntilDoneExposureSelector() {
            Mock<IProject> mockProject = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            mockProject.SetupAllProperties();
            mockProject.SetupProperty(p => p.FilterSwitchFrequency, 0);
            mockProject.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            mockTarget.SetupAllProperties();

            IExposureSelector s = new ExposureSelectionExpert().GetExposureSelector(mockProject.Object, mockTarget.Object, new Target());
            (s is RepeatUntilDoneExposureSelector).Should().BeTrue();
        }

        [Test]
        public void testOverrideOrderExposureSelector() {
            Mock<IProject> mockProject = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            mockProject.SetupAllProperties();
            mockProject.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            mockTarget.SetupAllProperties();

            List<OverrideExposureOrderItem> oeos = new List<OverrideExposureOrderItem>();
            oeos.Add(new OverrideExposureOrderItem(101, 1, OverrideExposureOrderAction.Exposure, 0));
            oeos.Add(new OverrideExposureOrderItem(101, 2, OverrideExposureOrderAction.Dither, -1));

            Target target = new Target();
            target.OverrideExposureOrders = oeos;

            IExposureSelector s = new ExposureSelectionExpert().GetExposureSelector(mockProject.Object, mockTarget.Object, target);
            (s is OverrideOrderExposureSelector).Should().BeTrue();
        }
    }
}