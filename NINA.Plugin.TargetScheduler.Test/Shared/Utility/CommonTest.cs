using FluentAssertions;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NUnit.Framework;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Shared.Utility {

    [TestFixture]
    public class CommonTest {

        [Test]
        public void testNotEmulator() {
            // prevent commits with emulator on
            Common.USE_EMULATOR.Should().BeFalse();
        }

        [Test]
        public void testListEmpty() {
            Common.IsEmpty(null).Should().BeTrue();
            Common.IsEmpty(new List<int>()).Should().BeTrue();
            Common.IsEmpty(new List<int>() { 0 }).Should().BeFalse();
        }

        [Test]
        public void testListNotEmpty() {
            Common.IsNotEmpty(null).Should().BeFalse();
            Common.IsNotEmpty(new List<int>()).Should().BeFalse();
            Common.IsNotEmpty(new List<int>() { 0 }).Should().BeTrue();
        }
    }
}