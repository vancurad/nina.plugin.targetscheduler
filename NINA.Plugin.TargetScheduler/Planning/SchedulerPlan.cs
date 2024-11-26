using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class SchedulerPlan {
        public DateTime PlanTime { get; private set; }

        //public List<IProject> Projects { get; private set; }
        public ITarget PlanTarget { get; private set; }

        public List<IningInstruction> PlanInstructions { get; private set; }
        public DateTime? WaitForNextTargetTime { get; private set; }
        public bool IsEmulator { get; set; }
        public string DetailsLog { get; private set; }

        public SchedulerPlan(DateTime planTime, ITarget planTarget, List<IningInstruction> planInstructions, bool logPlan) {
            this.PlanTime = planTime;
            //this.Projects = projects;
            this.PlanTarget = planTarget;
            this.PlanInstructions = planInstructions;
            this.WaitForNextTargetTime = null;

            /* TODO
            if (logPlan) {
                string log = LogPlanResults();
                DetailsLog = DetailsLog + log;
                TSLogger.Info(log);
            }
            */
        }
    }
}