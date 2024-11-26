using FluentAssertions;
using NINA.Plugin.TargetScheduler.Astrometry;
using NINA.Plugin.TargetScheduler.Test.Planning;
using NUnit.Framework;
using System;

namespace NINA.Plugin.TargetScheduler.Test.Astrometry {

    [TestFixture]
    public class TwilightCircumstancesTest {

        [Test]
        public void testNorthMid() {
            DateTime dateTime = new DateTime(2024, 12, 1, 12, 0, 0);
            DateTime date = dateTime.Date;

            var sut = new TwilightCircumstances(TestData.North_Mid_Lat, dateTime);

            Assertions.AssertTime(sut.CivilTwilightStart, date, 17, 4, 19);
            Assertions.AssertTime(sut.NauticalTwilightStart, date, 17, 33, 31);
            Assertions.AssertTime(sut.AstronomicalTwilightStart, date, 18, 4, 39);
            Assertions.AssertTime(sut.NighttimeStart, date, 18, 35, 10);
            date = date.AddDays(1);
            Assertions.AssertTime(sut.NighttimeEnd, date, 5, 35, 55);
            Assertions.AssertTime(sut.AstronomicalTwilightEnd, date, 6, 6, 23);
            Assertions.AssertTime(sut.NauticalTwilightEnd, date, 6, 37, 39);
            Assertions.AssertTime(sut.CivilTwilightEnd, date, 7, 6, 57);

            sut.HasCivilTwilight().Should().BeTrue();
            sut.HasNauticalTwilight().Should().BeTrue();
            sut.HasAstronomicalTwilight().Should().BeTrue();
            sut.HasNighttime().Should().BeTrue();

            sut.GetTwilightSpan(TwilightLevel.Civil).Should().NotBeNull();
            sut.GetTwilightSpan(TwilightLevel.Nautical).Should().NotBeNull();
            sut.GetTwilightSpan(TwilightLevel.Astronomical).Should().NotBeNull();
            sut.GetTwilightSpan(TwilightLevel.Nighttime).Should().NotBeNull();
        }

        [Test]
        public void testSouthMid() {
            DateTime dateTime = new DateTime(2024, 6, 1, 12, 0, 0);
            DateTime date = dateTime.Date;

            var sut = new TwilightCircumstances(TestData.South_Mid_Lat, dateTime);

            Assertions.AssertTime(sut.CivilTwilightStart, date, 18, 16, 1);
            Assertions.AssertTime(sut.NauticalTwilightStart, date, 18, 45, 14);
            Assertions.AssertTime(sut.AstronomicalTwilightStart, date, 19, 16, 40);
            Assertions.AssertTime(sut.NighttimeStart, date, 19, 47, 16);
            date = date.AddDays(1);
            Assertions.AssertTime(sut.NighttimeEnd, date, 6, 48, 58);
            Assertions.AssertTime(sut.AstronomicalTwilightEnd, date, 7, 19, 41);
            Assertions.AssertTime(sut.NauticalTwilightEnd, date, 7, 51, 2);
            Assertions.AssertTime(sut.CivilTwilightEnd, date, 8, 20, 4);

            sut.HasCivilTwilight().Should().BeTrue();
            sut.HasNauticalTwilight().Should().BeTrue();
            sut.HasAstronomicalTwilight().Should().BeTrue();
            sut.HasNighttime().Should().BeTrue();

            sut.GetTwilightSpan(TwilightLevel.Civil).Should().NotBeNull();
            sut.GetTwilightSpan(TwilightLevel.Nautical).Should().NotBeNull();
            sut.GetTwilightSpan(TwilightLevel.Astronomical).Should().NotBeNull();
            sut.GetTwilightSpan(TwilightLevel.Nighttime).Should().NotBeNull();
        }

        [Test]
        public void testAbovePolarCircleSummer() {
            DateTime dateTime = new DateTime(2024, 6, 21, 12, 0, 0);

            // Sun doesn't set on the summer solstice ...
            var sut = new TwilightCircumstances(TestData.North_Artic, dateTime);

            sut.CivilTwilightStart.Should().BeNull();
            sut.CivilTwilightEnd.Should().BeNull();
            sut.NauticalTwilightStart.Should().BeNull();
            sut.NauticalTwilightEnd.Should().BeNull();
            sut.AstronomicalTwilightStart.Should().BeNull();
            sut.AstronomicalTwilightEnd.Should().BeNull();
            sut.NighttimeStart.Should().BeNull();
            sut.NighttimeEnd.Should().BeNull();

            sut.HasCivilTwilight().Should().BeFalse();
            sut.HasNauticalTwilight().Should().BeFalse();
            sut.HasAstronomicalTwilight().Should().BeFalse();
            sut.HasNighttime().Should().BeFalse();

            sut.GetTwilightSpan(TwilightLevel.Civil).Should().BeNull();
            sut.GetTwilightSpan(TwilightLevel.Nautical).Should().BeNull();
            sut.GetTwilightSpan(TwilightLevel.Astronomical).Should().BeNull();
            sut.GetTwilightSpan(TwilightLevel.Nighttime).Should().BeNull();
        }

        [Test]
        public void testWayAbovePolarCircleWinter() {
            DateTime dateTime = new DateTime(2024, 12, 21, 12, 0, 0);
            DateTime date = dateTime.Date;

            // Sun doesn't rise on the winter solstice but it's still twilight most of the day ...
            var sut = new TwilightCircumstances(TestData.North_Artic_80, dateTime);

            sut.CivilTwilightStart.Should().BeNull();
            sut.CivilTwilightEnd.Should().BeNull();
            sut.NauticalTwilightStart.Should().BeNull();
            sut.NauticalTwilightEnd.Should().BeNull();
            sut.AstronomicalTwilightStart.Should().BeNull();
            sut.AstronomicalTwilightEnd.Should().BeNull();

            Assertions.AssertTime(sut.NighttimeStart, date, 16, 20, 36);
            date = date.AddDays(1);
            Assertions.AssertTime(sut.NighttimeEnd, date, 8, 16, 8);

            sut.HasCivilTwilight().Should().BeFalse();
            sut.HasNauticalTwilight().Should().BeFalse();
            sut.HasAstronomicalTwilight().Should().BeFalse();
            sut.HasNighttime().Should().BeTrue();

            sut.GetTwilightSpan(TwilightLevel.Civil).Should().BeNull();
            sut.GetTwilightSpan(TwilightLevel.Nautical).Should().BeNull();
            sut.GetTwilightSpan(TwilightLevel.Astronomical).Should().BeNull();

            var ti = sut.GetTwilightSpan(TwilightLevel.Nighttime);
            date = dateTime.Date;
            Assertions.AssertTime(ti.StartTime, date, 16, 20, 36);
            date = date.AddDays(1);
            Assertions.AssertTime(ti.EndTime, date, 8, 16, 8);
        }
    }
}