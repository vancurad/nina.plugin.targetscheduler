using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class ImageDataTest {

        [Test]
        public void TestDefaults() {
            ImageData sut = new ImageData("tag", null, 1);
            sut.Tag.Should().Be("tag");
            sut.Data.Should().BeNull();
            sut.AcquiredImageId.Should().Be(1);
        }
    }
}