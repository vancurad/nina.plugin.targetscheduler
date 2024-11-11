using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;
using System;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class AcquiredImageTest {

        [Test]
        public void TestAcquiredImage() {
            // AcquiredImage(string profileId, int projectId, int targetId, DateTime acquiredDate, string filterName, bool accepted, string rejectReason, ImageMetadata imageMetadata) {
            DateTime now = new DateTime(2024, 11, 11, 1, 2, 3);
            AcquiredImage sut = new AcquiredImage("abc123", 1, 2, now, "Ha", true, "foo", null);
            sut.ProfileId.Should().Be("abc123");
            sut.ProjectId.Should().Be(1);
            sut.TargetId.Should().Be(2);
            sut.AcquiredDate.Should().Be(now);
            sut.FilterName.Should().Be("Ha");
            sut.Accepted.Should().BeTrue();
            sut.RejectReason.Should().Be("foo");
            sut.Metadata.Should().BeNull();
        }
    }
}