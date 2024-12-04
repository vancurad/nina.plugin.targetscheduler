using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Entities {

    [TestFixture]
    public class PlanningOverrideExposureOrderTest {

        [Test]
        public void TestOverrideExposureOrder() {
            OverrideExposureOrderItem oeo = new OverrideExposureOrderItem();
            oeo.Order = 1;
            oeo.Action = OverrideExposureOrderAction.Exposure;
            oeo.ReferenceIdx = 2;

            PlanningOverrideExposureOrder sut = new PlanningOverrideExposureOrder(oeo);
            sut.Order.Should().Be(1);
            sut.Action.Should().Be(OverrideExposureOrderAction.Exposure);
            sut.ReferenceIdx.Should().Be(2);

            oeo = new OverrideExposureOrderItem();
            oeo.TargetId = 202;
            oeo.Order = 2;
            oeo.Action = OverrideExposureOrderAction.Dither;
            oeo.ReferenceIdx = -1;

            sut = new PlanningOverrideExposureOrder(oeo);
            sut.Order.Should().Be(2);
            sut.Action.Should().Be(OverrideExposureOrderAction.Dither);
            sut.ReferenceIdx.Should().Be(-1);
        }
    }
}