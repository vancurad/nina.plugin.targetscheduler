using FluentAssertions;
using NINA.Core.Model.Equipment;
using NINA.Plugin.TargetScheduler.Flats;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Flats {

    [TestFixture]
    public class LightSessionTest {

        [Test]
        public void TestLightSession() {
            FlatsExpert fe = new FlatsExpert();

            FlatSpec fs1 = new FlatSpec(1, "Ha", 10, 20, new BinningMode(1, 1), 0, 123.4, 89);
            FlatSpec fs2 = new FlatSpec(1, "OIII", 10, 20, new BinningMode(1, 1), 0, 123.4, 89);
            FlatSpec fs3 = new FlatSpec(1, "SII", 10, 20, new BinningMode(1, 1), 0, 123.4, 89);
            FlatSpec fs4 = new FlatSpec(1, "Red", 10, 20, new BinningMode(1, 1), 0, ImageMetadata.NO_ROTATOR_ANGLE, 89);
            FlatSpec fs5 = new FlatSpec(1, "Red", 10, 20, new BinningMode(1, 1), 0, ImageMetadata.NO_ROTATOR_ANGLE, 100);

            DateTime sd1 = fe.GetLightSessionDate(DateTime.Now.Date.AddHours(18));
            DateTime sd2 = fe.GetLightSessionDate(DateTime.Now.Date.AddDays(2).AddHours(18));
            DateTime sd3 = fe.GetLightSessionDate(DateTime.Now.Date.AddDays(3).AddHours(18));

            LightSession ls1 = new LightSession(1, sd1, 1, fs1);
            ls1.Equals(ls1).Should().BeTrue();
            LightSession ls2 = new LightSession(1, sd1, 1, fs1);
            ls1.Equals(ls2).Should().BeTrue();

            ls2 = new LightSession(1, sd1, 2, fs1);
            ls1.Equals(ls2).Should().BeFalse();

            ls2 = new LightSession(2, sd1, 1, fs1);
            ls1.Equals(ls2).Should().BeFalse();
            ls2 = new LightSession(2, sd1, 1, fs2);
            ls1.Equals(ls2).Should().BeFalse();

            ls1 = new LightSession(1, sd2, 1, fs1);
            ls2 = new LightSession(1, sd2, 1, fs1);
            ls1.Equals(ls2).Should().BeTrue();
            ls2 = new LightSession(2, sd1, 1, fs1);
            ls1.Equals(ls2).Should().BeFalse();

            List<LightSession> list = new List<LightSession> { ls1, ls2 };
            list.Contains(ls1).Should().BeTrue();
            list.Contains(ls2).Should().BeTrue();
            LightSession ls3 = new LightSession(3, sd3, 1, fs3);
            list.Contains(ls3).Should().BeFalse();
            list.Add(ls3);
            list.Contains(ls3).Should().BeTrue();

            LightSession ls4 = new LightSession(1, sd1, 1, fs4);
            ls4.Equals(ls4).Should().BeTrue();
            LightSession ls5 = new LightSession(1, sd1, 1, fs4);
            ls4.Equals(ls5).Should().BeTrue();

            ls5 = new LightSession(2, sd1, 1, fs4);
            ls4.Equals(ls5).Should().BeFalse();

            ls4 = new LightSession(2, sd2, 1, fs4);
            ls5 = new LightSession(2, sd2, 1, fs4);
            ls4.Equals(ls5).Should().BeTrue();

            ls5 = new LightSession(2, sd1, 1, fs4);
            ls4.Equals(ls5).Should().BeFalse();

            ls5 = new LightSession(2, sd2, 1, fs5);
            ls4.Equals(ls5).Should().BeFalse();
        }
    }
}