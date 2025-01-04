using NINA.Core.Model;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Sequencer.SequenceItem;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NINA.Plugin.TargetScheduler.Sequencer {

    /// <summary>
    /// Instances of this instruction are injected into the set of plan instructions in order to maintain
    /// the runtime progress display in the TS Container sequence UI.
    /// </summary>
    public class PlanSchedulerProgress : SequenceItem {
        private SchedulerProgressVM schedulerProgress;
        private IInstruction nextInstruction;

        public PlanSchedulerProgress(SchedulerProgressVM schedulerProgress, IInstruction instruction) {
            this.schedulerProgress = schedulerProgress;
            this.nextInstruction = instruction;
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            switch (nextInstruction) {
                case PlanSlew:
                    ITarget target = ((PlanSlew)nextInstruction).target;
                    schedulerProgress.End();
                    schedulerProgress.TargetStart(target.Project.Name, target.Name);
                    schedulerProgress.Add("Slew");
                    break;

                case PlanBeforeNewTargetContainer:
                    schedulerProgress.Add("BeforeTarget");
                    break;

                case PlanDither:
                    schedulerProgress.Add("Dither");
                    break;

                case PlanSwitchFilter:
                    schedulerProgress.Add("SwitchFilter", nextInstruction.exposure.FilterName);
                    break;

                case PlanSetReadoutMode:
                    schedulerProgress.Add("SetReadoutMode");
                    break;

                case Planning.Entities.PlanTakeExposure:
                    schedulerProgress.Add("TakeExposure", nextInstruction.exposure.FilterName);
                    break;
            }

            return Task.CompletedTask;
        }

        public override object Clone() {
            throw new NotImplementedException();
        }
    }
}