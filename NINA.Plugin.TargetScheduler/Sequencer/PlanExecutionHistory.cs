using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Sequencer {

    public class PlanExecutionHistory {
        private List<PlanExecutionHistoryItem> items;

        public void Add(PlanExecutionHistoryItem item) {
            if (items == null) { items = new List<PlanExecutionHistoryItem>(); }
            items.Add(item);
        }

        public void Clear() {
            if (items != null) {
                items.Clear();
                items = null;
            }
        }

        /// <summary>
        /// Find the most recent set of take exposure instructions that apply to the current target.  We
        /// search backwards from the end of the list gathering all exposure instructions as long as the
        /// target doesn't change or a wait is encountered.
        ///
        /// This is used to support the immediate flats instruction.
        /// </summary>
        public (ITarget, List<Planning.Entities.PlanTakeExposure>) GetImmediateTargetExposures() {
            List<Planning.Entities.PlanTakeExposure> exposures = new List<Planning.Entities.PlanTakeExposure>();

            if (Common.IsEmpty(items)) return (null, exposures);

            SchedulerPlan lastPlan = items[items.Count - 1].Plan;
            if (lastPlan.IsWait) return (null, exposures);

            for (int i = items.Count - 1; i >= 0; i--) {
                PlanExecutionHistoryItem item = items[i];
                if (item.Plan.IsWait) break;
                if (!item.Plan.PlanTarget.Equals(lastPlan.PlanTarget)) break;

                item.Plan.PlanInstructions.ForEach(instruction => {
                    if (instruction is Planning.Entities.PlanTakeExposure) {
                        exposures.Add((Planning.Entities.PlanTakeExposure)instruction);
                    }
                });
            }

            return (lastPlan.PlanTarget, exposures);
        }
    }

    public class PlanExecutionHistoryItem {
        private DateTime startTime;
        private DateTime endTime;
        private SchedulerPlan plan;

        public DateTime StartTime { get => startTime; private set => startTime = value; }
        public DateTime EndTime { get => endTime; set => endTime = value; }
        public SchedulerPlan Plan { get => plan; private set => plan = value; }

        public PlanExecutionHistoryItem(DateTime startTime, SchedulerPlan plan) {
            this.startTime = startTime;
            this.plan = plan;
        }
    }
}