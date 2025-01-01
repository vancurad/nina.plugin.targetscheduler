using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Test.Astrometry;
using NUnit.Framework;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Planning {

    [TestFixture]
    public class InstructionGeneratorTest {

        [Test]
        public void testGenerateNewTarget() {
            Mock<IExposure> mockExposure = PlanMocks.GetMockPlanExposure("L", 10, 0);
            Mock<ITarget> mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);

            mockExposure.SetupAllProperties();
            mockExposure.SetupProperty(x => x.PreDither, false);
            mockExposure.SetupProperty(x => x.FilterName, "L");

            mockTarget.SetupAllProperties();
            mockTarget.SetupProperty(t => t.SelectedExposure, mockExposure.Object);

            InstructionGenerator sut = new InstructionGenerator();
            List<IInstruction> list = sut.Generate(mockTarget.Object, null);

            list.Should().NotBeEmpty();
            (list[0] is PlanSlew).Should().BeTrue();
            (list[1] is PlanBeforeNewTargetContainer).Should().BeTrue();
            (list[2] is PlanSwitchFilter).Should().BeTrue();
            list[2].exposure.FilterName.Should().Be("L");
            (list[3] is PlanSetReadoutMode).Should().BeTrue();
            (list[4] is PlanTakeExposure).Should().BeTrue();
            (list[5] is PlanPostExposure).Should().BeTrue();

            mockExposure.SetupProperty(x => x.PreDither, true);
            list = sut.Generate(mockTarget.Object, null);

            list.Should().NotBeEmpty();
            (list[0] is PlanSlew).Should().BeTrue();
            (list[1] is PlanBeforeNewTargetContainer).Should().BeTrue();
            (list[2] is PlanDither).Should().BeTrue();
            (list[3] is PlanSwitchFilter).Should().BeTrue();
            list[3].exposure.FilterName.Should().Be("L");
            (list[4] is PlanSetReadoutMode).Should().BeTrue();
            (list[5] is PlanTakeExposure).Should().BeTrue();
            (list[6] is PlanPostExposure).Should().BeTrue();
        }

        [Test]
        public void testGenerateSameTarget() {
            Mock<IExposure> mockExposure = PlanMocks.GetMockPlanExposure("L", 10, 0);
            Mock<ITarget> mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);

            mockExposure.SetupAllProperties();
            mockExposure.SetupProperty(x => x.PreDither, false);
            mockExposure.SetupProperty(x => x.FilterName, "L");

            mockTarget.SetupAllProperties();
            mockTarget.SetupProperty(t => t.SelectedExposure, mockExposure.Object);

            InstructionGenerator sut = new InstructionGenerator();
            List<IInstruction> list = sut.Generate(mockTarget.Object, mockTarget.Object);

            list.Should().NotBeEmpty();
            (list[0] is PlanSwitchFilter).Should().BeTrue();
            list[0].exposure.FilterName.Should().Be("L");
            (list[1] is PlanSetReadoutMode).Should().BeTrue();
            (list[2] is PlanTakeExposure).Should().BeTrue();
            (list[3] is PlanPostExposure).Should().BeTrue();

            mockExposure.SetupProperty(x => x.PreDither, true);
            list = sut.Generate(mockTarget.Object, mockTarget.Object);

            list.Should().NotBeEmpty();
            (list[0] is PlanDither).Should().BeTrue();
            (list[1] is PlanSwitchFilter).Should().BeTrue();
            list[1].exposure.FilterName.Should().Be("L");
            (list[2] is PlanSetReadoutMode).Should().BeTrue();
            (list[3] is PlanTakeExposure).Should().BeTrue();
            (list[4] is PlanPostExposure).Should().BeTrue();
        }
    }
}