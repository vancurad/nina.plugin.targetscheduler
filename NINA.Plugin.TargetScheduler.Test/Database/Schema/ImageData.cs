using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class ImageDataTest {

        [Test]
        public void TestDefaults() {
            ImageData sut = new ImageData("tag", null, 1, 2, 3);
            sut.Tag.Should().Be("tag");
            sut.Data.Should().BeNull();
            sut.AcquiredImageId.Should().Be(1);
            sut.Width.Should().Be(2);
            sut.Height.Should().Be(3);
        }
    }
}