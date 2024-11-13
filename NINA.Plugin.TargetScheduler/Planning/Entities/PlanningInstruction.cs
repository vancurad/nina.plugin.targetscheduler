using NINA.Plugin.TargetScheduler.Controls.DatabaseManager;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Planning.Entities {

    public class PlanningInstruction : IPlanningInstruction {
        public IExposure planExposure { get; set; }

        public PlanningInstruction(IExposure planExposure) {
            this.planExposure = planExposure;
        }

        public static string InstructionsSummary(List<IPlanningInstruction> instructions) {
            if (instructions?.Count == 0) {
                return "";
            }

            Dictionary<string, int> exposures = new Dictionary<string, int>();
            StringBuilder order = new StringBuilder();
            foreach (IPlanningInstruction instruction in instructions) {
                if (instruction is PlanTakeExposure) {
                    string filterName = instruction.planExposure.FilterName;
                    order.Append(filterName);
                    if (exposures.ContainsKey(filterName)) {
                        exposures[filterName]++;
                    } else {
                        exposures.Add(filterName, 1);
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Order: {order}");
            foreach (KeyValuePair<string, int> entry in exposures) {
                sb.AppendLine($"{entry.Key}: {entry.Value}");
            }

            foreach (IPlanningInstruction instruction in instructions) {
                if (instruction is PlanWait) {
                    sb.AppendLine($"Wait until {Utils.FormatDateTimeFull(((PlanWait)instruction).waitForTime)}");
                }
            }

            return sb.ToString();
        }
    }

    public class PlanMessage : PlanningInstruction {
        public string msg { get; set; }

        public PlanMessage(string msg) : base(null) {
            this.msg = msg;
        }

        public override string ToString() {
            return $"Message: {msg}";
        }
    }

    public class PlanSlew : PlanningInstruction {
        public bool center { get; private set; }

        public PlanSlew(bool center) : base(null) {
            this.center = center;
        }

        public override string ToString() {
            return $"Slew: and center={center}";
        }
    }

    public class PlanSwitchFilter : PlanningInstruction {

        public PlanSwitchFilter(IExposure planExposure) : base(planExposure) {
        }

        public override string ToString() {
            return $"SwitchFilter: {planExposure.FilterName}";
        }
    }

    public class PlanSetReadoutMode : PlanningInstruction {

        public PlanSetReadoutMode(IExposure planExposure) : base(planExposure) {
        }

        public override string ToString() {
            return $"Set readoutmode: mode={planExposure.ReadoutMode}";
        }
    }

    public class PlanTakeExposure : PlanningInstruction {

        public PlanTakeExposure(IExposure planExposure) : base(planExposure) {
        }

        public override string ToString() {
            return $"TakeExposure: {planExposure.FilterName} {planExposure.ExposureLength}";
        }
    }

    public class PlanDither : PlanningInstruction {

        public PlanDither() : base(null) {
        }

        public override string ToString() {
            return "Dither";
        }
    }

    public class PlanBeforeNewTargetContainer : PlanningInstruction {

        public PlanBeforeNewTargetContainer() : base(null) {
        }

        public override string ToString() {
            return "BeforeNewTargetContainer";
        }
    }

    public class PlanWait : PlanningInstruction {
        public DateTime waitForTime { get; set; }

        public PlanWait(DateTime waitForTime) : base(null) {
            this.waitForTime = waitForTime;
        }

        public override string ToString() {
            return $"Wait: {Utils.FormatDateTimeFull(waitForTime)}";
        }
    }

    public class PlanOverrideItem {
        public bool IsDither { get; private set; }
        public IExposure PlanExposure { get; private set; }

        public PlanOverrideItem() {
            IsDither = true;
            PlanExposure = null;
        }

        public PlanOverrideItem(IExposure planExposure) {
            IsDither = false;
            PlanExposure = planExposure;
        }

        public override string ToString() {
            return IsDither ? OverrideExposureOrder.DITHER : PlanExposure.FilterName;
        }
    }
}