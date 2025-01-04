using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class InstructionGenerator {

        public InstructionGenerator() {
        }

        public List<IInstruction> Generate(ITarget target, ITarget previousTarget) {
            List<IInstruction> instructions = new List<IInstruction>();
            IExposure selectedExposure = target.SelectedExposure;

            // If this target is different from the previous, add the slew and 'Before Target' instructions
            if (!target.Equals(previousTarget)) {
                instructions.Add(new PlanSlew(target, true));
                instructions.Add(new PlanBeforeNewTargetContainer());
            }

            if (selectedExposure.PreDither) {
                instructions.Add(new PlanDither());
            }

            instructions.Add(new PlanSwitchFilter(selectedExposure));
            instructions.Add(new PlanSetReadoutMode(selectedExposure));
            instructions.Add(new PlanTakeExposure(selectedExposure));
            instructions.Add(new PlanPostExposure(target));

            return instructions;
        }
    }
}