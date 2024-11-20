using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class OverrideExposureOrderTest {

        [Test]
        public void TestDefaults() {
            var sut = new OverrideExposureOrder(1, OverrideExposureOrderAction.Exposure, 202);
            sut.Order.Should().Be(1);
            sut.Action.Should().Be(OverrideExposureOrderAction.Exposure);
            sut.ReferenceIdx.Should().Be(202);

            sut = new OverrideExposureOrder(2, OverrideExposureOrderAction.Dither);
            sut.Order.Should().Be(2);
            sut.Action.Should().Be(OverrideExposureOrderAction.Dither);
            sut.ReferenceIdx.Should().Be(-1);
        }
    }
}