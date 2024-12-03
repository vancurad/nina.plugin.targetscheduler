using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Test.Astrometry;
using NUnit.Framework;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Planning {

    [TestFixture]
    public class FilterCadenceExpertTest {

        [Test]
        public void testGenerateInitialPreexisting() {
            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            pp1.SetupProperty(m => m.FilterSwitchFrequency, 1);

            List<IFilterCadence> filterCadences = new List<IFilterCadence>();
            filterCadences.Add(new PlanningFilterCadence(1, true, FilterCadenceAction.Exposure, 0));

            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("IC1805", TestData.IC1805);
            pt.SetupProperty(t => t.FilterCadences, filterCadences);
            SetEPs(pt);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            // Existing list will be left in place
            FilterCadenceExpert sut = new FilterCadenceExpert(pp1.Object, pt.Object);
            sut.GenerateInitial().Count.Should().Be(1);
        }

        [Test]
        public void testGenerateInitialFSFZero() {
            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            pp1.SetupProperty(m => m.FilterSwitchFrequency, 0);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("IC1805", TestData.IC1805);
            SetEPs(pt);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            // Filter switch frequency = 0 => empty initial list
            FilterCadenceExpert sut = new FilterCadenceExpert(pp1.Object, pt.Object);
            sut.GenerateInitial().Count.Should().Be(0);
        }

        [Test]
        public void testGenerateInitialSmart() {
            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            pp1.SetupProperty(m => m.FilterSwitchFrequency, 1);
            pp1.SetupProperty(m => m.SmartExposureOrder, true);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("IC1805", TestData.IC1805);
            SetEPs(pt);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            // Smart exposure order => empty initial list
            pp1.SetupProperty(m => m.FilterSwitchFrequency, 1);
            pp1.SetupProperty(m => m.SmartExposureOrder, true);
            FilterCadenceExpert sut = new FilterCadenceExpert(pp1.Object, pt.Object);
            sut.GenerateInitial().Count.Should().Be(0);
        }

        [Test]
        public void testGenerateInitialFSFPlus() {
            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("IC1805", TestData.IC1805);
            SetEPs(pt);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            // FSF > 0
            pp1.SetupProperty(m => m.FilterSwitchFrequency, 2);
            pp1.SetupProperty(m => m.DitherEvery, 0);
            pp1.SetupProperty(m => m.SmartExposureOrder, false);
            FilterCadenceExpert sut = new FilterCadenceExpert(pp1.Object, pt.Object);
            var list = sut.GenerateInitial();
            list.Should().HaveCount(8);

            AssertFilterCadence(list[0], 1, true, FilterCadenceAction.Exposure, 0);
            AssertFilterCadence(list[1], 2, false, FilterCadenceAction.Exposure, 0);
            AssertFilterCadence(list[2], 3, false, FilterCadenceAction.Exposure, 1);
            AssertFilterCadence(list[3], 4, false, FilterCadenceAction.Exposure, 1);
            AssertFilterCadence(list[4], 5, false, FilterCadenceAction.Exposure, 2);
            AssertFilterCadence(list[5], 6, false, FilterCadenceAction.Exposure, 2);
            AssertFilterCadence(list[6], 7, false, FilterCadenceAction.Exposure, 3);
            AssertFilterCadence(list[7], 8, false, FilterCadenceAction.Exposure, 3);
        }

        [Test]
        public void testAutoDither() {
            Mock<IProject> pp = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("IC1805", TestData.IC1805);
            SetEPs(pt);
            PlanMocks.AddMockPlanTarget(pp, pt);

            pp.SetupProperty(m => m.FilterSwitchFrequency, 2);
            pp.SetupProperty(m => m.DitherEvery, 1);
            pp.SetupProperty(m => m.SmartExposureOrder, false);
            FilterCadenceExpert sut = new FilterCadenceExpert(pp.Object, pt.Object);
            var list = pt.Object.FilterCadences;

            // LLRRGGBB => LdLRdRGdGBdB
            list.Should().HaveCount(12);
            AssertFilterCadence(list[0], 1, true, FilterCadenceAction.Exposure, 0);
            AssertFilterCadence(list[1], 2, false, FilterCadenceAction.Dither, -1);
            AssertFilterCadence(list[2], 3, false, FilterCadenceAction.Exposure, 0);
            AssertFilterCadence(list[3], 4, false, FilterCadenceAction.Exposure, 1);
            AssertFilterCadence(list[4], 5, false, FilterCadenceAction.Dither, -1);
            AssertFilterCadence(list[5], 6, false, FilterCadenceAction.Exposure, 1);
            AssertFilterCadence(list[6], 7, false, FilterCadenceAction.Exposure, 2);
            AssertFilterCadence(list[7], 8, false, FilterCadenceAction.Dither, -1);
            AssertFilterCadence(list[8], 9, false, FilterCadenceAction.Exposure, 2);
            AssertFilterCadence(list[9], 10, false, FilterCadenceAction.Exposure, 3);
            AssertFilterCadence(list[10], 11, false, FilterCadenceAction.Dither, -1);
            AssertFilterCadence(list[11], 12, false, FilterCadenceAction.Exposure, 3);

            pp.SetupProperty(p => p.FilterSwitchFrequency, 3);
            pp.SetupProperty(p => p.DitherEvery, 2);
            pt.SetupProperty(t => t.FilterCadences, new List<IFilterCadence>());
            sut = new FilterCadenceExpert(pp.Object, pt.Object);
            list = pt.Object.FilterCadences;

            // LLLRRRGGGBBB => LLdLRRdRGGdGBBdB
            list.Should().HaveCount(16);
            AssertFilterCadence(list[0], 1, true, FilterCadenceAction.Exposure, 0);
            AssertFilterCadence(list[1], 2, false, FilterCadenceAction.Exposure, 0);
            AssertFilterCadence(list[2], 3, false, FilterCadenceAction.Dither, -1);
            AssertFilterCadence(list[3], 4, false, FilterCadenceAction.Exposure, 0);
            AssertFilterCadence(list[4], 5, false, FilterCadenceAction.Exposure, 1);
            AssertFilterCadence(list[5], 6, false, FilterCadenceAction.Exposure, 1);
            AssertFilterCadence(list[6], 7, false, FilterCadenceAction.Dither, -1);
            AssertFilterCadence(list[7], 8, false, FilterCadenceAction.Exposure, 1);
            AssertFilterCadence(list[8], 9, false, FilterCadenceAction.Exposure, 2);
            AssertFilterCadence(list[9], 10, false, FilterCadenceAction.Exposure, 2);
            AssertFilterCadence(list[10], 11, false, FilterCadenceAction.Dither, -1);
            AssertFilterCadence(list[11], 12, false, FilterCadenceAction.Exposure, 2);
            AssertFilterCadence(list[12], 13, false, FilterCadenceAction.Exposure, 3);
            AssertFilterCadence(list[13], 14, false, FilterCadenceAction.Exposure, 3);
            AssertFilterCadence(list[14], 15, false, FilterCadenceAction.Dither, -1);
            AssertFilterCadence(list[15], 16, false, FilterCadenceAction.Exposure, 3);
        }

        [Test]
        public void testGenerateInitialOverride() {
            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            pp1.SetupProperty(m => m.FilterSwitchFrequency, 0);
            pp1.SetupProperty(m => m.SmartExposureOrder, false);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("IC1805", TestData.IC1805);
            SetEPs(pt);
            List<IOverrideExposureOrder> overrideExposureOrders = new List<IOverrideExposureOrder>();
            overrideExposureOrders.Add(new PlanningOverrideExposureOrder(1, OverrideExposureOrderAction.Exposure, 0));
            overrideExposureOrders.Add(new PlanningOverrideExposureOrder(2, OverrideExposureOrderAction.Exposure, 1));
            overrideExposureOrders.Add(new PlanningOverrideExposureOrder(3, OverrideExposureOrderAction.Dither, -1));
            pt.SetupProperty(t => t.OverrideExposureOrders, overrideExposureOrders);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            FilterCadenceExpert sut = new FilterCadenceExpert(pp1.Object, pt.Object);
            var list = pt.Object.FilterCadences;
            list.Should().HaveCount(3);

            AssertFilterCadence(list[0], 1, true, FilterCadenceAction.Exposure, 0);
            AssertFilterCadence(list[1], 2, false, FilterCadenceAction.Exposure, 1);
            AssertFilterCadence(list[2], 3, false, FilterCadenceAction.Dither, -1);
        }

        [Test]
        public void testGetNext() {
            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            pp1.SetupProperty(m => m.FilterSwitchFrequency, 2);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("IC1805", TestData.IC1805);
            SetEPs(pt);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            pp1.SetupProperty(m => m.FilterSwitchFrequency, 2);
            pp1.SetupProperty(m => m.SmartExposureOrder, false);
            FilterCadenceExpert sut = new FilterCadenceExpert(pp1.Object, pt.Object);
            var list = pt.Object.FilterCadences;
            list.Should().HaveCount(8);

            IFilterCadence fc = sut.GetNext();
            fc.Order.Should().Be(1);
            fc.ReferenceIdx.Should().Be(0);
            IExposure pe = sut.GetExposurePlanForFilterCadence(fc);
            pe.Should().NotBeNull();
            pe.FilterName.Should().Be("L");

            PlanningFilterCadence pfc = (PlanningFilterCadence)list[0];
            pfc.Next = false;
            pfc = (PlanningFilterCadence)list[4];
            pfc.Next = true;
            fc = sut.GetNext();
            fc.Order.Should().Be(5);
            fc.ReferenceIdx.Should().Be(2);
            pe = sut.GetExposurePlanForFilterCadence(fc);
            pe.Should().NotBeNull();
            pe.FilterName.Should().Be("G");
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

        private void AssertFilterCadence(IFilterCadence fc, int order, bool next, FilterCadenceAction action, int refIdx) {
            fc.Order.Should().Be(order);
            fc.Next.Should().Be(next);
            fc.Action.Should().Be(action);
            fc.ReferenceIdx.Should().Be(refIdx);
        }
    }
}