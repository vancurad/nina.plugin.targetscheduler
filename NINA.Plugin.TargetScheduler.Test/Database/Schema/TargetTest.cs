using FluentAssertions;
using NINA.Astrometry;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class TargetTest {

        [Test]
        public void TestDefaults() {
            Target sut = new Target();
            sut.active.Should().BeTrue();
            sut.RA.Should().Be(0);
            sut.Dec.Should().Be(0);
            sut.Epoch.Should().Be(Epoch.J2000);
            sut.Rotation.Should().Be(0);
            sut.ROI.Should().Be(100);
            sut.OverrideExposureOrder.Should().BeNull();
            sut.ExposurePlans.Count.Should().Be(0);
        }

        [Test]
        public void TestGetPasteCopy() {
            Target target = new Target();
            target.Name = "MyTarget";
            Target sut = target.GetPasteCopy("abc123");
            sut.Name.Should().Be("MyTarget (1)");
            sut.active.Should().BeTrue();
            sut.RA.Should().Be(0);
            sut.Dec.Should().Be(0);
            sut.Epoch.Should().Be(Epoch.J2000);
            sut.Rotation.Should().Be(0);
            sut.ROI.Should().Be(100);
            sut.OverrideExposureOrder.Should().BeNull();
            sut.ExposurePlans.Count.Should().Be(0);
        }

        [Test]
        public void TestCoordinatesGetSet() {
            Target sut = new Target();
            sut.Enabled.Should().Be(true);

            sut.RAHours = 5;
            sut.RAMinutes = 10;
            sut.RASeconds = 20;
            sut.DecDegrees = -10;
            sut.DecMinutes = 10;
            sut.DecSeconds = 20;

            sut.Coordinates.ToString().Should().Be("RA: 05:10:20; Dec: -10° 10' 20\"; Epoch: J2000");
            sut.ra.Should().BeApproximately(5.1722, 0.001);
            sut.RA.Should().BeApproximately(5.1722, 0.001);
            sut.dec.Should().BeApproximately(-10.1722, 0.001);
            sut.Dec.Should().BeApproximately(-10.1722, 0.001);
        }
    }
}