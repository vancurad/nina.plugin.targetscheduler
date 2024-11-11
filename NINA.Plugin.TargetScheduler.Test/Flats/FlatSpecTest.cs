using FluentAssertions;
using NINA.Core.Model.Equipment;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Flats;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Flats {

    [TestFixture]
    public class FlatSpecTest {

        [Test]
        public void TestFlatsSpec() {
            FlatSpec fs1 = new FlatSpec(1, "Ha", 10, 20, new BinningMode(2, 2), 0, 123.4, 89);
            fs1.FilterName.Should().Be("Ha");
            fs1.Gain.Should().Be(10);
            fs1.Offset.Should().Be(20);
            fs1.BinningMode.X.Should().Be(2);
            fs1.ReadoutMode.Should().Be(0);
            fs1.Rotation.Should().Be(123.4);
            fs1.ROI.Should().Be(89);
            fs1.Key.Should().Be("Ha_10_20_2x2_0_89");

            ImageMetadata imageMetaData = FlatsExpertTest.GetImageMetadata(1, "Ha", 10, 20, "2x2", 0, 123.4, 89);
            AcquiredImage acquiredImage = new AcquiredImage(imageMetaData);
            acquiredImage.FilterName = imageMetaData.FilterName;
            FlatSpec fs2 = new FlatSpec(1, acquiredImage);
            fs2.FilterName.Should().Be("Ha");
            fs2.Gain.Should().Be(10);
            fs2.Offset.Should().Be(20);
            fs2.BinningMode.X.Should().Be(2);
            fs2.ReadoutMode.Should().Be(0);
            fs2.Rotation.Should().Be(123.4);
            fs2.ROI.Should().Be(89);
            fs2.Key.Should().Be("Ha_10_20_2x2_0_89");

            fs1.Equals(fs2).Should().BeTrue();

            FlatSpec fs3 = new FlatSpec(1, "Ha", 10, 20, new BinningMode(2, 2), 0, ImageMetadata.NO_ROTATOR_ANGLE, 89);
            fs3.FilterName.Should().Be("Ha");
            fs3.Gain.Should().Be(10);
            fs3.Offset.Should().Be(20);
            fs3.BinningMode.X.Should().Be(2);
            fs3.ReadoutMode.Should().Be(0);
            fs3.Rotation.Should().Be(0);
            fs3.ROI.Should().Be(89);
            fs3.Key.Should().Be("Ha_10_20_2x2_0_89");

            imageMetaData = FlatsExpertTest.GetImageMetadata(1, "Ha", 11, 20, "2x2", 0, ImageMetadata.NO_ROTATOR_ANGLE, 89);
            acquiredImage = new AcquiredImage(imageMetaData);
            acquiredImage.FilterName = imageMetaData.FilterName;
            FlatSpec fs4 = new FlatSpec(1, acquiredImage);
            fs4.FilterName.Should().Be("Ha");
            fs4.Gain.Should().Be(11);
            fs4.Offset.Should().Be(20);
            fs4.BinningMode.X.Should().Be(2);
            fs4.ReadoutMode.Should().Be(0);
            fs4.Rotation.Should().Be(0);
            fs4.ROI.Should().Be(89);
            fs4.Key.Should().Be("Ha_11_20_2x2_0_89");

            fs1.Equals(fs3).Should().BeTrue();
            fs2.Equals(fs3).Should().BeTrue();
            fs2.Equals(fs4).Should().BeFalse();
            fs3.Equals(fs4).Should().BeFalse();

            FlatSpec fs5 = new FlatSpec(1, "Lum", 10, 20, new BinningMode(1, 1), 0, 123.4, 100);
            FlatSpec fs6 = new FlatSpec(1, "Lum", 10, 20, new BinningMode(1, 1), 0, 33.3, 100);
            FlatSpec fs7 = new FlatSpec(2, "Lum", 10, 20, new BinningMode(1, 1), 0, 123.4, 100);

            fs5.Equals(fs6).Should().BeTrue();
            fs5.Equals(fs7).Should().BeTrue();
        }

        [Test]
        public void TestFlatsSpecEquals() {
            FlatSpec fs1 = new FlatSpec(1, "Ha", 10, 20, new BinningMode(2, 2), 0, 0, 100);
            FlatSpec fs2 = new FlatSpec(2, "Ha", 10, 20, new BinningMode(2, 2), 0, 0, 100);
            fs1.Equals(fs2).Should().BeTrue(); // different targets but otherwise same

            FlatSpec fs3 = new FlatSpec(1, "Ha", 10, 20, new BinningMode(2, 2), 0, 10, 100);
            fs1.Equals(fs3).Should().BeTrue(); // same target but different rotation - rotation ignored for same target
            fs2.Equals(fs3).Should().BeFalse(); // different target and different rotation

            FlatSpec fs4 = new FlatSpec(1, "O3", 10, 20, new BinningMode(2, 2), 0, 0, 100);
            fs1.Equals(fs4).Should().BeFalse(); // same target and rotation but different filter

            FlatSpec fs5 = new FlatSpec(1, "O3", 10, 20, new BinningMode(2, 2), 0, 10, 100);
            fs1.Equals(fs5).Should().BeFalse(); // same target, different rotation but different filter
        }
    }
}