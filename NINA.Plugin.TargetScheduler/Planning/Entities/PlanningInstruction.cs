using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Planning.Entities {

    public class PlanningInstruction : IInstruction {
        public IExposure exposure { get; set; }

        public PlanningInstruction(IExposure exposure) {
            this.exposure = exposure;
        }

        public static string InstructionsSummary(List<IInstruction> instructions) {
            if (instructions?.Count == 0) {
                return "";
            }

            Dictionary<string, int> exposures = new Dictionary<string, int>();
            StringBuilder order = new StringBuilder();
            foreach (IInstruction instruction in instructions) {
                if (instruction is PlanTakeExposure) {
                    string filterName = instruction.exposure.FilterName;
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

            foreach (IInstruction instruction in instructions) {
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
        public ITarget target { get; private set; }
        public bool center { get; private set; }

        public PlanSlew(ITarget target, bool center) : base(null) {
            this.target = target;
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
            return $"SwitchFilter: {exposure.FilterName}";
        }
    }

    public class PlanSetReadoutMode : PlanningInstruction {

        public PlanSetReadoutMode(IExposure planExposure) : base(planExposure) {
        }

        public override string ToString() {
            return $"Set readoutmode: mode={exposure.ReadoutMode}";
        }
    }

    public class PlanTakeExposure : PlanningInstruction {

        public PlanTakeExposure(IExposure planExposure) : base(planExposure) {
        }

        public override string ToString() {
            return $"TakeExposure: {exposure.FilterName} {exposure.ExposureLength}";
        }
    }

    public class PlanPostExposure : PlanningInstruction {
        public ITarget target { get; private set; }

        public PlanPostExposure(ITarget target) : base(target.SelectedExposure) {
            this.target = target;
        }

        public override string ToString() {
            return $"PostExposure: {target.Name} {exposure.FilterName}";
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
        public IExposure exposure { get; private set; }

        public PlanOverrideItem() {
            IsDither = true;
            exposure = null;
        }

        public PlanOverrideItem(IExposure exposure) {
            IsDither = false;
            this.exposure = exposure;
        }

        public override string ToString() {
            return IsDither ? OverrideExposureOrderAction.Dither.ToString() : exposure.FilterName;
        }
    }
}