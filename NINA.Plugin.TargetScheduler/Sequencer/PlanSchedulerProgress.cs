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
                    schedulerProgress.Add(SchedulerProgressVM.SlewLabel);
                    break;

                case PlanBeforeNewTargetContainer:
                    schedulerProgress.Add(SchedulerProgressVM.BeforeTargetLabel);
                    break;

                case PlanDither:
                    schedulerProgress.Add(SchedulerProgressVM.DitherLabel);
                    break;

                case PlanSwitchFilter:
                    schedulerProgress.Add(SchedulerProgressVM.SwitchFilterLabel, nextInstruction.exposure.FilterName);
                    break;

                case PlanSetReadoutMode:
                    break;

                case Planning.Entities.PlanTakeExposure:
                    schedulerProgress.Add(SchedulerProgressVM.TakeExposureLabel, nextInstruction.exposure.FilterName);
                    break;
            }

            return Task.CompletedTask;
        }

        public override object Clone() {
            throw new NotImplementedException();
        }
    }
}