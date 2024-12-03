using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Entities {

    [TestFixture]
    public class PlanningFilterCadenceTest {

        [Test]
        public void TestFilterCadence() {
            FilterCadence filterCadence = new FilterCadence();
            filterCadence.Order = 1;
            filterCadence.Action = FilterCadenceAction.Exposure;
            filterCadence.Next = true;
            filterCadence.ReferenceIdx = 2;

            PlanningFilterCadence sut = new PlanningFilterCadence(filterCadence);
            sut.Order.Should().Be(1);
            sut.Action.Should().Be(FilterCadenceAction.Exposure);
            sut.Next.Should().BeTrue();
            sut.ReferenceIdx.Should().Be(2);

            filterCadence = new FilterCadence();
            filterCadence.Order = 2;
            filterCadence.Action = FilterCadenceAction.Dither;
            filterCadence.Next = false;
            filterCadence.ReferenceIdx = -1;

            sut = new PlanningFilterCadence(filterCadence);
            sut.Order.Should().Be(2);
            sut.Action.Should().Be(FilterCadenceAction.Dither);
            sut.Next.Should().BeFalse();
            sut.ReferenceIdx.Should().Be(-1);

            sut = new PlanningFilterCadence(3, false, FilterCadenceAction.Exposure, 22);
            sut.Order.Should().Be(3);
            sut.Next.Should().BeFalse();
            sut.Action.Should().Be(FilterCadenceAction.Exposure);
            sut.ReferenceIdx.Should().Be(22);

            sut = new PlanningFilterCadence(sut, 8, true);
            sut.Order.Should().Be(8);
            sut.Next.Should().BeTrue();
            sut.Action.Should().Be(FilterCadenceAction.Exposure);
            sut.ReferenceIdx.Should().Be(22);
        }
    }
}