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
    public class BasicExposureSelectorTest {

        [Test]
        public void testBasicExposureSelector() {
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            pp.SetupAllProperties();
            pp.SetupProperty(p => p.FilterSwitchFrequency, 2);
            pp.SetupProperty(p => p.DitherEvery, 1);
            pp.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            pt.SetupProperty(t => t.Project, pp.Object);
            SetEPs(pt);
            FilterCadence fc = new FilterCadenceFactory().Generate(pp.Object, pt.Object, new Target());
            pt.SetupProperty(t => t.FilterCadence, fc);
            DitherManager dm = new DitherManager(1);
            pt.SetupProperty(t => t.DitherManager, dm);

            BasicExposureSelector sut = new BasicExposureSelector();
            IExposure e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();
            dm.Reset();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeFalse();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeTrue();
            dm.Reset();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("G");
            e.PreDither.Should().BeFalse();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("G");
            e.PreDither.Should().BeTrue();
            dm.Reset();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("B");
            e.PreDither.Should().BeFalse();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("B");
            e.PreDither.Should().BeTrue();
            dm.Reset();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();
        }

        [Test]
        public void testForOverDither() {
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            pp.SetupAllProperties();
            pp.SetupProperty(p => p.FilterSwitchFrequency, 2);
            pp.SetupProperty(p => p.DitherEvery, 1);
            pp.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            pt.SetupProperty(t => t.Project, pp.Object);
            SetEPs(pt);
            FilterCadence fc = new FilterCadenceFactory().Generate(pp.Object, pt.Object, new Target());
            pt.SetupProperty(t => t.FilterCadence, fc);
            DitherManager dm = new DitherManager(1);
            pt.SetupProperty(t => t.DitherManager, dm);

            BasicExposureSelector sut = new BasicExposureSelector();
            IExposure e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();
            dm.Reset();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeFalse();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeTrue();
            dm.Reset();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("G");
            e.PreDither.Should().BeFalse();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("G");
            e.PreDither.Should().BeTrue();
            dm.Reset();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("B");
            e.PreDither.Should().BeFalse();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("B");
            e.PreDither.Should().BeTrue();
            dm.Reset();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();
            dm.AddExposure(e);

            pt.Object.ExposurePlans.ForEach(e => { if (e.FilterName != "L") e.Rejected = true; });
            dm.Reset();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();
            dm.Reset();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();
            dm.Reset();
            dm.AddExposure(e);

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();
        }

        [Test]
        public void testAllExposurePlansRejected() {
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            pp.SetupAllProperties();
            pp.SetupProperty(p => p.FilterSwitchFrequency, 2);
            pp.SetupProperty(p => p.DitherEvery, 1);
            pp.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            pt.SetupProperty(t => t.Project, pp.Object);
            SetEPs(pt);
            pt.Object.ExposurePlans.ForEach(e => { e.Rejected = true; });

            FilterCadence fc = new FilterCadenceFactory().Generate(pp.Object, pt.Object, new Target());
            pt.SetupProperty(t => t.FilterCadence, fc);

            BasicExposureSelector sut = new BasicExposureSelector();
            Action select = () => sut.Select(new DateTime(2024, 12, 1), pp.Object, pt.Object, null);
            select.Should().Throw<Exception>().WithMessage("unexpected: all exposure plans were rejected at exposure selection time for target 'T1' at time 12/1/2024 12:00:00 AM");
        }

        [Test]
        public void testNoFilterCadence() {
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            pp.SetupAllProperties();
            pp.SetupProperty(p => p.FilterSwitchFrequency, 2);
            pp.SetupProperty(p => p.DitherEvery, 1);
            pp.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            pt.SetupProperty(t => t.Project, pp.Object);
            SetEPs(pt);

            BasicExposureSelector sut = new BasicExposureSelector();
            Action select = () => sut.Select(new DateTime(2024, 12, 1), pp.Object, pt.Object, null);
            select.Should().Throw<Exception>().WithMessage("unexpected: empty filter cadence for target 'T1' at time 12/1/2024 12:00:00 AM");
        }

        private void SetEPs(Mock<ITarget> pt) {
            Mock<IExposure> Lpf = PlanMocks.GetMockPlanExposure("L", 10, 0);
            Mock<IExposure> Rpf = PlanMocks.GetMockPlanExposure("R", 10, 0);
            Mock<IExposure> Gpf = PlanMocks.GetMockPlanExposure("G", 10, 0);
            Mock<IExposure> Bpf = PlanMocks.GetMockPlanExposure("B", 10, 0);

            PlanMocks.AddMockPlanFilter(pt, Lpf);
            PlanMocks.AddMockPlanFilter(pt, Rpf);
            PlanMocks.AddMockPlanFilter(pt, Gpf);
            PlanMocks.AddMockPlanFilter(pt, Bpf);
        }
    }
}