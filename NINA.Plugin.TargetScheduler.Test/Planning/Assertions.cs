using FluentAssertions;
using NUnit.Framework;
using System;

namespace NINA.Plugin.TargetScheduler.Test.Planning {

    public class Assertions {

        public static void AssertTime(DateTime expected, DateTime? actual, int hours, int minutes, int seconds) {
            actual.Should().NotBeNull();

            DateTime edt = expected.Date.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds);
            DateTime adt = new DateTime(((DateTime)actual).Year, ((DateTime)actual).Month, ((DateTime)actual).Day,
                ((DateTime)actual).Hour, ((DateTime)actual).Minute, ((DateTime)actual).Second);

            bool cond = (edt == adt);
            if (!cond) {
                TestContext.WriteLine($"assertTime failed:");
                TestContext.WriteLine($"  expected: {edt}");
                TestContext.WriteLine($"  actual:   {adt}");
            }

            cond.Should().BeTrue();
        }
    }
}