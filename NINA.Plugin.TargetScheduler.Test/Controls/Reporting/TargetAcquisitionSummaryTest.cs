using FluentAssertions;
using NINA.Plugin.TargetScheduler.Controls.Reporting;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Grading;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NUnit.Framework;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Controls.Reporting {

    [TestFixture]
    public class TargetAcquisitionSummaryTest {

        [Test]
        public void testBasic() {
            Target target = new Target();
            List<AcquiredImage> list = new List<AcquiredImage>();
            list.Add(GetAI("Lum", 300, GradingStatus.Accepted));
            TargetAcquisitionSummary sut = new TargetAcquisitionSummary(target, list);
            sut.Rows.Count.Should().Be(2);
            AssertRow(sut.Rows[0], TargetAcquisitionSummary.TOTAL_LBL, "0h 5m 0s", "0h 5m 0s", "0h 0m 0s", "0h 0m 0s");
            AssertRow(sut.Rows[1], "Lum", "0h 5m 0s", "0h 5m 0s", "0h 0m 0s", "0h 0m 0s");

            list.Add(GetAI("Lum", 300, GradingStatus.Rejected));
            sut = new TargetAcquisitionSummary(target, list);
            sut.Rows.Count.Should().Be(2);
            AssertRow(sut.Rows[0], TargetAcquisitionSummary.TOTAL_LBL, "0h 10m 0s", "0h 5m 0s", "0h 5m 0s", "0h 0m 0s");
            AssertRow(sut.Rows[1], "Lum", "0h 10m 0s", "0h 5m 0s", "0h 5m 0s", "0h 0m 0s");

            list.Add(GetAI("Lum", 300, GradingStatus.Pending));
            sut = new TargetAcquisitionSummary(target, list);
            sut.Rows.Count.Should().Be(2);
            AssertRow(sut.Rows[0], TargetAcquisitionSummary.TOTAL_LBL, "0h 15m 0s", "0h 5m 0s", "0h 5m 0s", "0h 5m 0s");
            AssertRow(sut.Rows[1], "Lum", "0h 15m 0s", "0h 5m 0s", "0h 5m 0s", "0h 5m 0s");

            list.Add(GetAI("Red", 180, GradingStatus.Accepted));
            sut = new TargetAcquisitionSummary(target, list);
            sut.Rows.Count.Should().Be(3);
            AssertRow(sut.Rows[0], TargetAcquisitionSummary.TOTAL_LBL, "0h 18m 0s", "0h 8m 0s", "0h 5m 0s", "0h 5m 0s");
            AssertRow(sut.Rows[1], "Lum", "0h 15m 0s", "0h 5m 0s", "0h 5m 0s", "0h 5m 0s");
            AssertRow(sut.Rows[2], "Red", "0h 3m 0s", "0h 3m 0s", "0h 0m 0s", "0h 0m 0s");

            list.Add(GetAI("Grn", 600, GradingStatus.Rejected));
            sut = new TargetAcquisitionSummary(target, list);
            sut.Rows.Count.Should().Be(4);
            AssertRow(sut.Rows[0], TargetAcquisitionSummary.TOTAL_LBL, "0h 28m 0s", "0h 8m 0s", "0h 15m 0s", "0h 5m 0s");
            AssertRow(sut.Rows[1], "Lum", "0h 15m 0s", "0h 5m 0s", "0h 5m 0s", "0h 5m 0s");
            AssertRow(sut.Rows[2], "Red", "0h 3m 0s", "0h 3m 0s", "0h 0m 0s", "0h 0m 0s");
            AssertRow(sut.Rows[3], "Grn", "0h 10m 0s", "0h 0m 0s", "0h 10m 0s", "0h 0m 0s");

            for (int i = 0; i < 21; i++) {
                list.Add(GetAI("Blu", 610, GradingStatus.Pending));
            }

            sut = new TargetAcquisitionSummary(target, list);
            sut.Rows.Count.Should().Be(5);
            AssertRow(sut.Rows[0], TargetAcquisitionSummary.TOTAL_LBL, "4h 1m 30s", "0h 8m 0s", "0h 15m 0s", "3h 38m 30s");
            AssertRow(sut.Rows[1], "Lum", "0h 15m 0s", "0h 5m 0s", "0h 5m 0s", "0h 5m 0s");
            AssertRow(sut.Rows[2], "Red", "0h 3m 0s", "0h 3m 0s", "0h 0m 0s", "0h 0m 0s");
            AssertRow(sut.Rows[3], "Grn", "0h 10m 0s", "0h 0m 0s", "0h 10m 0s", "0h 0m 0s");
            AssertRow(sut.Rows[4], "Blu", "3h 33m 30s", "0h 0m 0s", "0h 0m 0s", "3h 33m 30s");
        }

        [Test]
        public void testEmpty() {
            TargetAcquisitionSummary sut = new TargetAcquisitionSummary(null, null);
            sut.Rows.Count.Should().Be(0);

            TargetAcquisitionRow row = new TargetAcquisitionRow(null, null);
            row.TotalTime.Should().BeNull();
            row.AcceptedTime.Should().BeNull();
            row.RejectedTime.Should().BeNull();
            row.PendingTime.Should().BeNull();
        }

        private AcquiredImage GetAI(string filterName, double duration, GradingStatus status) {
            AcquiredImage ai = new AcquiredImage(new ImageMetadata() { ExposureDuration = duration });
            ai.FilterName = filterName;
            ai.GradingStatus = status;
            return ai;
        }

        private void AssertRow(TargetAcquisitionRow row, string key, string tt, string at, string rt, string pt) {
            row.Key.Should().Be(key);
            row.TotalTime.Should().Be(tt);
            row.AcceptedTime.Should().Be(at);
            row.RejectedTime.Should().Be(rt);
            row.PendingTime.Should().Be(pt);
        }
    }
}