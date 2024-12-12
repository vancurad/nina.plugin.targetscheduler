using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Planning.Scoring.Rules;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Plugin.TargetScheduler.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class SchedulerPlan {
        public string PlanId { get; private set; }
        public DateTime PlanTime { get; private set; }

        public DateTime StartTime { get => PlanTime; }
        public DateTime EndTime { get; private set; }
        public TimeInterval TimeInterval { get; private set; }

        public ITarget PlanTarget { get; private set; }
        public List<IProject> Projects { get; private set; }

        public List<IInstruction> PlanInstructions { get; private set; }
        public DateTime? WaitForNextTargetTime { get; private set; }
        public bool IsEmulator { get; set; }
        public string DetailsLog { get; private set; }

        public SchedulerPlan(DateTime planTime, List<IProject> projects, ITarget planTarget, List<IInstruction> planInstructions, bool logPlan) {
            this.PlanId = Guid.NewGuid().ToString();
            this.PlanTime = planTime;
            this.PlanTarget = planTarget;
            this.Projects = projects;
            this.PlanInstructions = planInstructions;
            this.EndTime = planTime.AddSeconds(planTarget.SelectedExposure.ExposureLength);
            this.TimeInterval = new TimeInterval(StartTime, EndTime);
            this.WaitForNextTargetTime = null;

            if (logPlan) {
                string log = LogPlanResults();
                DetailsLog = DetailsLog + log;
                TSLogger.Info(log);
            }
        }

        public SchedulerPlan(DateTime planTime, List<IProject> projects, DateTime waitForNextTargetTime, bool logPlan) {
            this.PlanId = Guid.NewGuid().ToString();
            this.PlanTime = planTime;
            this.EndTime = waitForNextTargetTime;
            this.Projects = projects;
            this.TimeInterval = new TimeInterval(StartTime, EndTime);
            this.WaitForNextTargetTime = waitForNextTargetTime;

            if (logPlan) {
                string log = LogPlanResults();
                DetailsLog = DetailsLog + log;
                TSLogger.Info(log);
            }
        }

        // Stub version used on sync clients to support immediate flats
        public SchedulerPlan(ITarget planTarget) {
            this.PlanId = Guid.NewGuid().ToString();
            this.PlanTarget = planTarget;
            this.PlanInstructions = new List<IInstruction>();
            this.WaitForNextTargetTime = null;
        }

        public string LogPlanResults() {
            StringBuilder sb = new StringBuilder();
            string type = WaitForNextTargetTime.HasValue ? "WAIT" : "TARGET";

            sb.AppendLine("\n" + String.Format("{0,-6}", type) + " ==========================================================================================");

            if (type == "WAIT") {
                sb.AppendLine($"Plan Start:      {DateFmt(PlanTime)}");
                sb.AppendLine($"Wait Until:      {DateFmt(WaitForNextTargetTime)}");
            }

            if (type == "TARGET") {
                sb.AppendLine($"Selected Target: {PlanTarget.Project.Name}/{PlanTarget.Name}");
                sb.AppendLine($"Plan Start:      {DateFmt(PlanTime)}");
                sb.AppendLine($"Plan Stop:       {DateFmt(PlanTime.AddSeconds(TimeInterval.Duration))}");
                sb.AppendLine($"Hard Stop:       {DateFmt(PlanTarget.EndTime)} (target sets)");
            }

            bool haveScoringRuns = false;
            bool hasAllEPsRejected = false;

            if (Projects != null) {
                sb.AppendLine(String.Format("\n{0,-40} {1,-27} {2,6}   {3,19}", "TARGETS CONSIDERED", "REJECTED FOR", "SCORE", "POTENTIAL START"));
                foreach (IProject project in Projects) {
                    foreach (ITarget target in project.Targets) {
                        string score = "";
                        string startTime = GetStartTime(target);

                        if (target.ScoringResults != null && target.ScoringResults.Results.Count > 0) {
                            haveScoringRuns = true;
                            score = String.Format("{0:0.00}", target.ScoringResults.TotalScore * ScoringRule.WEIGHT_SCALE);
                        }

                        sb.AppendLine(String.Format("{0,-40} {1,-27} {2,6}   {3}", $"{project.Name}/{target.Name}", target.RejectedReason, score, startTime));
                        if (target.RejectedReason == Reasons.TargetAllExposurePlans) {
                            hasAllEPsRejected = true;
                        }
                    }
                }

                if (hasAllEPsRejected) {
                    sb.AppendLine("\n(Rejection for 'all exposure plans' is due to moon avoidance or all exposure plans complete.)");
                }

                if (haveScoringRuns) {
                    sb.AppendLine("\nSCORING RUNS");
                    foreach (IProject project in Projects) {
                        foreach (ITarget target in project.Targets) {
                            if (target.ScoringResults != null && target.ScoringResults.Results.Count > 0) {
                                sb.AppendLine($"\n{project.Name}/{target.Name}");
                                sb.AppendLine(String.Format("{0,-30} {1,-9} {2,11} {3,11}", "RULE", "RAW SCORE", "WEIGHT", "SCORE"));
                                foreach (RuleResult result in target.ScoringResults.Results) {
                                    sb.AppendLine(String.Format("{0,-30} {1,9:0.00} {2,10:0.00}%  {3,10:0.00}",
                                        result.ScoringRule.Name,
                                        result.Score * ScoringRule.WEIGHT_SCALE,
                                        result.Weight * ScoringRule.WEIGHT_SCALE,
                                        result.Score * result.Weight * ScoringRule.WEIGHT_SCALE));
                                }

                                sb.AppendLine("----------------------------------------------------------------");
                                sb.AppendLine(String.Format("{0,57} {1,6:0.00}", "TOTAL SCORE", target.ScoringResults.TotalScore * ScoringRule.WEIGHT_SCALE));
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }

        private string GetStartTime(ITarget target) {
            if (target.Rejected) {
                switch (target.RejectedReason) {
                    case Reasons.TargetNotYetVisible:
                    case Reasons.TargetBeforeMeridianWindow:
                    case Reasons.TargetLowerScore:
                        return DateFmt(target.StartTime);
                }
            }

            return "";
        }

        private string DateFmt(DateTime? dateTime) {
            if (dateTime == null || dateTime == DateTime.MinValue) {
                return "";
            }

            return ((DateTime)dateTime).ToString(Utils.DateFMT);
        }

        public string PlanSummary() {
            StringBuilder sb = new StringBuilder();
            if (WaitForNextTargetTime.HasValue) {
                sb.AppendLine($"Waiting until {Utils.FormatDateTimeFull(WaitForNextTargetTime)}");
            } else {
                sb.AppendLine($"Target:         {PlanTarget.Name} at {PlanTarget.Coordinates.RAString} {PlanTarget.Coordinates.DecString}");
                sb.AppendLine($"Imaging window: {TimeInterval}");
                sb.Append($"Instructions:   {PlanningInstruction.InstructionsSummary(PlanInstructions)}");
            }

            return sb.ToString();
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Id: {PlanId}");
            sb.Append("Target: ").AppendLine(PlanTarget != null ? PlanTarget.Name : null);
            sb.AppendLine($"Interval: {TimeInterval}");
            sb.AppendLine($"Wait: {WaitForNextTargetTime}");
            sb.AppendLine($"Instructions:\n");
            if (PlanInstructions != null) {
                foreach (IInstruction instruction in PlanInstructions) {
                    sb.AppendLine($"{instruction}");
                }
            }

            return sb.ToString();
        }
    }
}