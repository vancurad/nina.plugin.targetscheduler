using FluentAssertions;
using NINA.Core.Model.Equipment;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;
using System;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class FlatHistoryTest {

        [Test]
        public void TestDefaults() {
            DateTime lsd = new DateTime(2024, 11, 11, 1, 2, 3);
            DateTime ftd = new DateTime(2024, 11, 12, 2, 3, 4);
            BinningMode mode = new BinningMode(1, 1);
            FlatHistory sut = new FlatHistory(1, lsd, ftd, 2, "abc123", "type", "Ha", 10, 20, mode, 3, 123.4, 0.8);
            sut.TargetId.Should().Be(1);
            sut.LightSessionDate.Should().Be(lsd);
            sut.FlatsTakenDate.Should().Be(ftd);
            sut.LightSessionId.Should().Be(2);
            sut.ProfileId.Should().Be("abc123");
            sut.FlatsType.Should().Be("type");
            sut.FilterName.Should().Be("Ha");
            sut.Gain.Should().Be(10);
            sut.Offset.Should().Be(20);
            sut.BinningMode.Should().Be(mode);
            sut.ReadoutMode.Should().Be(3);
            sut.Rotation.Should().Be(123.4);
            sut.ROI.Should().Be(0.8);
        }
    }
}