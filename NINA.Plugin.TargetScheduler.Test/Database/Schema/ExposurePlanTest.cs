using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class ExposurePlanTest {

        [Test]
        public void TestDefaults() {
            ExposurePlan sut = new ExposurePlan("abc123");
            sut.ProfileId.Should().Be("abc123");
            sut.Exposure.Should().Be(-1);
            sut.Desired.Should().Be(1);
            sut.Acquired.Should().Be(0);
            sut.Accepted.Should().Be(0);
        }

        [Test]
        public void TestGetPasteCopy() {
            ExposurePlan ep = new ExposurePlan("abc123");
            ExposurePlan sut = ep.GetPasteCopy("def456");
            sut.ProfileId.Should().Be("def456");
            sut.Exposure.Should().Be(-1);
            sut.Desired.Should().Be(1);
            sut.Acquired.Should().Be(0);
            sut.Accepted.Should().Be(0);
        }
    }
}