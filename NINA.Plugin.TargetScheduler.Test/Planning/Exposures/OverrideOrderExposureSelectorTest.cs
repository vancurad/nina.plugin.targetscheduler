using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Exposures;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Test.Astrometry;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Exposures {

    [TestFixture]
    public class OverrideOrderExposureSelectorTest {

        [Test]
        public void testBasic() {
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            pp.SetupAllProperties();
            pp.SetupProperty(p => p.FilterSwitchFrequency, 100);
            pp.SetupProperty(p => p.DitherEvery, 100);
            pp.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            pt.SetupProperty(t => t.Project, pp.Object);
            SetEPs(pt);

            List<IOverrideExposureOrderItem> oeos = new List<IOverrideExposureOrderItem>();
            oeos.Add(new PlanningOverrideExposureOrder(1, OverrideExposureOrderAction.Exposure, 0));
            oeos.Add(new PlanningOverrideExposureOrder(2, OverrideExposureOrderAction.Exposure, 0));
            oeos.Add(new PlanningOverrideExposureOrder(3, OverrideExposureOrderAction.Dither, -1));
            oeos.Add(new PlanningOverrideExposureOrder(4, OverrideExposureOrderAction.Exposure, 1));
            oeos.Add(new PlanningOverrideExposureOrder(5, OverrideExposureOrderAction.Exposure, 1));
            oeos.Add(new PlanningOverrideExposureOrder(6, OverrideExposureOrderAction.Dither, -1));
            pt.SetupProperty(t => t.OverrideExposureOrders, oeos);

            FilterCadence fc = new FilterCadenceFactory().Generate(pp.Object, pt.Object, new Target());
            pt.SetupProperty(t => t.FilterCadence, fc);

            OverrideOrderExposureSelector sut = new OverrideOrderExposureSelector();
            IExposure e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeTrue();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeFalse();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeTrue();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("R");
            e.PreDither.Should().BeFalse();
        }

        [Test]
        public void testOne() {
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            pp.SetupAllProperties();
            pp.SetupProperty(p => p.FilterSwitchFrequency, 100);
            pp.SetupProperty(p => p.DitherEvery, 100);
            pp.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            pt.SetupProperty(t => t.Project, pp.Object);
            SetEPs(pt);

            List<IOverrideExposureOrderItem> oeos = new List<IOverrideExposureOrderItem>();
            oeos.Add(new PlanningOverrideExposureOrder(1, OverrideExposureOrderAction.Exposure, 0));
            pt.SetupProperty(t => t.OverrideExposureOrders, oeos);

            FilterCadence fc = new FilterCadenceFactory().Generate(pp.Object, pt.Object, new Target());
            pt.SetupProperty(t => t.FilterCadence, fc);

            OverrideOrderExposureSelector sut = new OverrideOrderExposureSelector();
            IExposure e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();
        }

        [Test]
        public void testOneDither() {
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("P1", ProjectState.Active);
            pp.SetupAllProperties();
            pp.SetupProperty(p => p.FilterSwitchFrequency, 100);
            pp.SetupProperty(p => p.DitherEvery, 100);
            pp.SetupProperty(p => p.SmartExposureOrder, false);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            pt.SetupProperty(t => t.Project, pp.Object);
            SetEPs(pt);

            List<IOverrideExposureOrderItem> oeos = new List<IOverrideExposureOrderItem>();
            oeos.Add(new PlanningOverrideExposureOrder(1, OverrideExposureOrderAction.Exposure, 0));
            oeos.Add(new PlanningOverrideExposureOrder(2, OverrideExposureOrderAction.Dither, -1));
            pt.SetupProperty(t => t.OverrideExposureOrders, oeos);

            FilterCadence fc = new FilterCadenceFactory().Generate(pp.Object, pt.Object, new Target());
            pt.SetupProperty(t => t.FilterCadence, fc);

            OverrideOrderExposureSelector sut = new OverrideOrderExposureSelector();
            IExposure e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeFalse();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();

            fc.Advance();
            e = sut.Select(DateTime.Now, pp.Object, pt.Object, null);
            e.FilterName.Should().Be("L");
            e.PreDither.Should().BeTrue();
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