using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Grading;
using NUnit.Framework;
using System;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class AcquiredImageTest {

        [Test]
        public void TestAcquiredImage() {
            DateTime now = new DateTime(2024, 11, 11, 1, 2, 3);
            AcquiredImage sut = new AcquiredImage("abc123", 1, 2, 3, now, "Ha", GradingStatus.Accepted, "foo", null);
            sut.ProfileId.Should().Be("abc123");
            sut.ProjectId.Should().Be(1);
            sut.TargetId.Should().Be(2);
            sut.ExposureId.Should().Be(3);
            sut.AcquiredDate.Should().Be(now);
            sut.FilterName.Should().Be("Ha");
            sut.GradingStatus.Should().Be(GradingStatus.Accepted);
            sut.Accepted.Should().BeTrue();
            sut.Rejected.Should().BeFalse();
            sut.Pending.Should().BeFalse();
            sut.RejectReason.Should().Be("foo");
            sut.Metadata.Should().BeNull();
        }
    }
}