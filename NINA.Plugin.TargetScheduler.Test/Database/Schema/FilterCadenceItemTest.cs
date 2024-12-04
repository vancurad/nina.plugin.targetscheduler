using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class FilterCadenceItemTest {

        [Test]
        public void TestDefaults() {
            var sut = new FilterCadenceItem(101, 1, false, FilterCadenceAction.Exposure, 202);
            sut.TargetId.Should().Be(101);
            sut.Order.Should().Be(1);
            sut.Next.Should().BeFalse();
            sut.Action.Should().Be(FilterCadenceAction.Exposure);
            sut.ReferenceIdx.Should().Be(202);

            sut = new FilterCadenceItem(202, 2, true, FilterCadenceAction.Dither);
            sut.TargetId.Should().Be(202);
            sut.Order.Should().Be(2);
            sut.Next.Should().BeTrue();
            sut.Action.Should().Be(FilterCadenceAction.Dither);
            sut.ReferenceIdx.Should().Be(-1);
        }
    }
}