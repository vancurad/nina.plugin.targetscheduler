using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Test.Astrometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Planning {

    [TestFixture]
    public class SchedulerPlanTest {

        [Test]
        public void testTarget() {
            DateTime atTime = new(2024, 12, 1, 20, 0, 0);
            Mock<ITarget> mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            Mock<IExposure> mockExposure = new Mock<IExposure>();
            mockExposure.SetupAllProperties();
            mockExposure.SetupProperty(x => x.ExposureLength, 300);
            mockTarget.SetupAllProperties();
            mockTarget.SetupProperty(t => t.SelectedExposure, mockExposure.Object);

            SchedulerPlan sut = new(atTime, new List<IProject>(), mockTarget.Object, new List<IInstruction>(), false);
            sut.PlanTime.Should().Be(atTime);
            sut.StartTime.Should().Be(atTime);
            sut.EndTime.Should().Be(atTime.AddSeconds(300));
            sut.PlanTarget.Should().NotBeNull();
            sut.Projects.Count.Should().Be(0);
            sut.TimeInterval.StartTime.Should().Be(atTime);
            sut.TimeInterval.EndTime.Should().Be(atTime.AddSeconds(300));
            sut.PlanInstructions.Count.Should().Be(0);
        }

        [Test]
        public void testWait() {
            DateTime atTime = new(2024, 12, 1, 20, 0, 0);
            SchedulerPlan sut = new(atTime, new List<IProject>(), atTime.AddHours(1), false);
            sut.PlanTime.Should().Be(atTime);
            sut.StartTime.Should().Be(atTime);
            sut.EndTime.Should().Be(atTime.AddHours(1));
            sut.PlanTarget.Should().BeNull();
            sut.Projects.Count.Should().Be(0);
            sut.TimeInterval.StartTime.Should().Be(atTime);
            sut.TimeInterval.EndTime.Should().Be(atTime.AddHours(1));
            sut.PlanInstructions.Should().BeNull();
        }
    }
}