using NINA.Astrometry;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Exposures;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Planning.Scoring.Rules;
using NINA.Plugin.TargetScheduler.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Planning.Entities {

    public class PlanningTarget : ITarget {
        public string PlanId { get; set; }
        public int DatabaseId { get; set; }
        public string Name { get; set; }
        public Coordinates Coordinates { get; set; }
        public Epoch Epoch { get; set; }
        public double Rotation { get; set; }
        public double ROI { get; set; }
        public List<IExposure> ExposurePlans { get; set; }
        public List<IExposure> CompletedExposurePlans { get; set; }
        public List<IOverrideExposureOrderItem> OverrideExposureOrders { get; set; }
        public FilterCadence FilterCadence { get; set; }
        public IProject Project { get; set; }
        public bool Rejected { get; set; }
        public string RejectedReason { get; set; }
        public IExposure SelectedExposure { get; set; }
        public ScoringResults ScoringResults { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CulminationTime { get; set; }
        public TimeInterval MeridianWindow { get; set; }

        public PlanningTarget(IProject planProject, Target target) {
            this.PlanId = Guid.NewGuid().ToString();
            this.DatabaseId = target.Id;
            this.Name = target.Name;
            this.Coordinates = target.Coordinates;
            this.Epoch = target.Epoch;
            this.Rotation = target.Rotation;
            this.ROI = target.ROI;
            this.Project = planProject;
            this.Rejected = false;

            this.ExposurePlans = new List<IExposure>();
            this.CompletedExposurePlans = new List<IExposure>();
            ExposureCompletionHelper helper = planProject.ExposureCompletionHelper;

            foreach (ExposurePlan plan in GetActiveExposurePlans(target)) {
                PlanningExposure planExposure = new PlanningExposure(this, plan, plan.ExposureTemplate);

                if (helper.IsIncomplete(planExposure)) {
                    this.ExposurePlans.Add(planExposure);
                } else {
                    this.CompletedExposurePlans.Add(planExposure);
                }
            }

            OverrideExposureOrders = new List<IOverrideExposureOrderItem>(target.OverrideExposureOrders.Count);
            target.OverrideExposureOrders.ForEach(oeo => { this.OverrideExposureOrders.Add(new PlanningOverrideExposureOrder(oeo)); });

            List<IFilterCadenceItem> filterCadences = new List<IFilterCadenceItem>(target.FilterCadences.Count);
            target.FilterCadences.ForEach(fc => { filterCadences.Add(new PlanningFilterCadence(fc)); });
            this.FilterCadence = new FilterCadence(filterCadences);
        }

        public PlanningTarget() {
        } // for PlanTargetEmulator only

        private List<ExposurePlan> GetActiveExposurePlans(Target target) {
            if (target.OverrideExposureOrders.Count == 0) {
                return target.ExposurePlans;
            }

            List<ExposurePlan> list = new List<ExposurePlan>();
            foreach (OverrideExposureOrderItem oeo in target.OverrideExposureOrders) {
                if (oeo.Action == OverrideExposureOrderAction.Dither) { continue; }
                if (oeo.Action == OverrideExposureOrderAction.Exposure) {
                    // TODO: fix this is wrong for EP idx approach
                    ExposurePlan exposurePlan = target.ExposurePlans.Find(ep => ep.Id == oeo.ReferenceIdx);
                    if (exposurePlan != null && !list.Contains(exposurePlan)) {
                        list.Add(exposurePlan);
                    }
                }
            }
            return list;
        }

        public void SetCircumstances(bool isVisible, DateTime startTime, DateTime culminationTime, DateTime endTime) {
            if (isVisible) {
                StartTime = startTime;
                CulminationTime = culminationTime;
                EndTime = endTime;
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Id: {PlanId}");
            sb.AppendLine($"Name: {Name}");
            sb.AppendLine($"Coords: {Coordinates.RAString} {Coordinates.DecString} {Epoch}");
            sb.AppendLine($"Rotation: {Rotation}");
            sb.AppendLine($"ROI: {ROI}");
            sb.AppendLine($"StartTime: {Utils.FormatDateTimeFull(StartTime)}");
            sb.AppendLine($"EndTime: {Utils.FormatDateTimeFull(EndTime)}");
            sb.AppendLine($"CulminationTime: {Utils.FormatDateTimeFull(CulminationTime)}");

            if (MeridianWindow != null) {
                sb.AppendLine($"Meridian Window: {Utils.FormatDateTimeFull(MeridianWindow.StartTime)} - {Utils.FormatDateTimeFull(MeridianWindow.EndTime)}");
            }

            sb.AppendLine($"Rejected: {Rejected}");
            sb.AppendLine($"RejectedReason: {RejectedReason}");

            sb.AppendLine("-- ExposurePlans:");
            foreach (PlanningExposure planExposure in ExposurePlans) {
                sb.AppendLine(planExposure.ToString());
            }

            return sb.ToString();
        }

        public override bool Equals(object obj) {
            if ((obj == null) || !this.GetType().Equals(obj.GetType())) {
                return false;
            }

            PlanningTarget p = (PlanningTarget)obj;
            return this.Name.Equals(p.Name) &&
                   this.Coordinates.RA.Equals(p.Coordinates.RA) &&
                   this.Coordinates.Dec.Equals(p.Coordinates.Dec) &&
                   this.Rotation.Equals(p.Rotation);
        }

        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + this.Name.GetHashCode();
            hash = hash * 23 + this.Coordinates.RAString.GetHashCode();
            hash = hash * 23 + this.Coordinates.DecString.GetHashCode();
            hash = hash * 23 + this.Rotation.ToString().GetHashCode();
            return hash;
        }
    }
}