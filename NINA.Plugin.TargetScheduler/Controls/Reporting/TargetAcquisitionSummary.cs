using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Grading;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Plugin.TargetScheduler.Util;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Controls.Reporting {

    public class TargetAcquisitionSummary {
        public static readonly string TOTAL_LBL = "Total";
        public static readonly int TOTAL = 0;
        public static readonly int ACCEPTED = 1;
        public static readonly int REJECTED = 2;
        public static readonly int PENDING = 3;

        public List<TargetAcquisitionRow> Rows { get; private set; }

        public TargetAcquisitionSummary(Target target, List<AcquiredImage> acquiredImages) {
            Rows = new List<TargetAcquisitionRow>();
            if (target == null || Common.IsEmpty(acquiredImages)) return;

            Dictionary<string, int[]> scratch = new Dictionary<string, int[]>();
            scratch[TOTAL_LBL] = new int[4];

            acquiredImages.ForEach(ai => {
                int duration = (int)ai.Metadata.ExposureDuration;
                int[] times;
                scratch.TryGetValue("Total", out times);
                times[TOTAL] += duration;

                switch (ai.GradingStatus) {
                    case GradingStatus.Accepted: times[ACCEPTED] += duration; break;
                    case GradingStatus.Rejected: times[REJECTED] += duration; break;
                    case GradingStatus.Pending: times[PENDING] += duration; break;
                }

                string filter = ai.FilterName;
                if (!scratch.TryGetValue(filter, out times)) {
                    times = new int[4];
                    scratch[filter] = times;
                }

                times[TOTAL] += duration;
                switch (ai.GradingStatus) {
                    case GradingStatus.Accepted: times[ACCEPTED] += duration; break;
                    case GradingStatus.Rejected: times[REJECTED] += duration; break;
                    case GradingStatus.Pending: times[PENDING] += duration; break;
                }
            });

            foreach (var (key, times) in scratch) {
                Rows.Add(new TargetAcquisitionRow(key, times));
            }
        }
    }

    public class TargetAcquisitionRow {
        public string Key { get; private set; }
        public string TotalTime { get; private set; }
        public string AcceptedTime { get; private set; }
        public string RejectedTime { get; private set; }
        public string PendingTime { get; private set; }

        public TargetAcquisitionRow(string key, int[] times) {
            if (string.IsNullOrEmpty(key) || Common.IsEmpty(times)) return;

            Key = key;
            TotalTime = Utils.StoHMS(times[TargetAcquisitionSummary.TOTAL]);
            AcceptedTime = Utils.StoHMS(times[TargetAcquisitionSummary.ACCEPTED]);
            RejectedTime = Utils.StoHMS(times[TargetAcquisitionSummary.REJECTED]);
            PendingTime = Utils.StoHMS(times[TargetAcquisitionSummary.PENDING]);
        }
    }
}