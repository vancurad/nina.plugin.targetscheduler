using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NUnit.Framework;
using System;

namespace NINA.Plugin.TargetScheduler.Test.Database.Schema {

    [TestFixture]
    public class RuleWeightTest {

        [Test]
        public void TestCtor() {
            RuleWeight sut = new RuleWeight("name", 1.234);
            sut.Name.Should().Be("name");
            sut.Weight.Should().Be(1.234);
        }

        [Test]
        public void TestBadWeight() {
            Action create = () => new RuleWeight("name", -1);
            create.Should().Throw<ArgumentException>().WithMessage("weight must be 0-100");
            create = () => new RuleWeight("name", 100.1);
            create.Should().Throw<ArgumentException>().WithMessage("weight must be 0-100");
        }
    }
}