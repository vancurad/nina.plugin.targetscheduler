using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class OverrideExposureOrderItemTest {

        [Test]
        public void TestDefaults() {
            var sut = new OverrideExposureOrderItem(101, 1, OverrideExposureOrderAction.Exposure, 202);
            sut.TargetId.Should().Be(101);
            sut.Order.Should().Be(1);
            sut.Action.Should().Be(OverrideExposureOrderAction.Exposure);
            sut.ReferenceIdx.Should().Be(202);

            sut = new OverrideExposureOrderItem(202, 2, OverrideExposureOrderAction.Dither);
            sut.TargetId.Should().Be(202);
            sut.Order.Should().Be(2);
            sut.Action.Should().Be(OverrideExposureOrderAction.Dither);
            sut.ReferenceIdx.Should().Be(-1);

            sut = sut.GetPasteCopy(303);
            sut.TargetId.Should().Be(303);
            sut.Order.Should().Be(2);
            sut.Action.Should().Be(OverrideExposureOrderAction.Dither);
            sut.ReferenceIdx.Should().Be(-1);
        }
    }
}