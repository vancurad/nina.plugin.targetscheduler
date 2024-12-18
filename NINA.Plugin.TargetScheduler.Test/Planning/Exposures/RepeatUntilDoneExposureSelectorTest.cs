using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Exposures;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Test.Astrometry;
using NUnit.Framework;
using System;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Exposures {

    [TestFixture]
    public class RepeatUntilDoneExposureSelectorTest {

        [Test]
        public void TestBasic() {
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            pp.SetupAllProperties();
            pp.SetupProperty(p => p.FilterSwitchFrequency, 2);
            pp.SetupProperty(p => p.DitherEvery, 1);
            pp.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            pt.SetupProperty(t => t.Project, pp.Object);
            SetEPs(pt);

            RepeatUntilDoneExposureSelector sut = new RepeatUntilDoneExposureSelector(pp.Object, pt.Object, new Target());
            pt.SetupProperty(t => t.ExposureSelector, sut);

            IExposure e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();
            sut.ExposureTaken(e);

            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();
            sut.ExposureTaken(e);
            Complete(e);

            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeFalse();
            sut.ExposureTaken(e);

            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeTrue();
            sut.ExposureTaken(e);

            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeTrue();
            sut.ExposureTaken(e);
            Complete(e);

            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("G");
            e.PreDither.Should().BeFalse();
            sut.ExposureTaken(e);

            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("G");
            e.PreDither.Should().BeTrue();
            sut.ExposureTaken(e);

            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("G");
            e.PreDither.Should().BeTrue();
            sut.ExposureTaken(e);
            Complete(e);

            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("B");
            e.PreDither.Should().BeFalse();
            sut.ExposureTaken(e);
        }

        private void Complete(IExposure e) {
            e.Rejected = true;
            e.RejectedReason = Reasons.FilterComplete;
        }

        private void SetEPs(Mock<ITarget> pt) {
            Mock<IExposure> Lpf = PlanMocks.GetMockPlanExposure("L", 2, 0);
            Mock<IExposure> Rpf = PlanMocks.GetMockPlanExposure("R", 2, 0);
            Mock<IExposure> Gpf = PlanMocks.GetMockPlanExposure("G", 2, 0);
            Mock<IExposure> Bpf = PlanMocks.GetMockPlanExposure("B", 2, 0);

            PlanMocks.AddMockPlanFilter(pt, Lpf);
            PlanMocks.AddMockPlanFilter(pt, Rpf);
            PlanMocks.AddMockPlanFilter(pt, Gpf);
            PlanMocks.AddMockPlanFilter(pt, Bpf);
        }
    }
}