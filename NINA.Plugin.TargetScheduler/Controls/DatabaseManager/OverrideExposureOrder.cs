using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Controls.DatabaseManager {

    public class OverrideExposureOrderOld {
        public static readonly string DITHER = "Dither";
        public static readonly char SEP = '|';

        /* What is this old code doing?
         * - Maintains the runtime list of items - will now just be on the target
         * - Can deserialize from DB OEO string to runtime items
         * - Can serialize from runtime list to DB string
         * - Does the remapping when a target is copied/pasted
         *
         * NONE of these are needed with new approach
         */

        private List<OverrideItemOld> overrideItems = new List<OverrideItemOld>();

        public List<OverrideItemOld> OverrideItems {
            get => overrideItems; set => overrideItems = value;
        }

        public OverrideExposureOrderOld(List<ExposurePlan> exposurePlans) {
            for (int i = 0; i < exposurePlans.Count; i++) {
                OverrideItems.Add(new OverrideItemOld(exposurePlans[i], exposurePlans[i].Id));
            }
        }

        public OverrideExposureOrderOld(string serialized, List<ExposurePlan> exposurePlans) {
            if (String.IsNullOrEmpty(serialized)) {
                return;
            }

            string[] items = serialized.Split(SEP);
            foreach (string item in items) {
                if (item == DITHER) {
                    OverrideItems.Add(new OverrideItemOld());
                } else {
                    int databaseId = 0;
                    Int32.TryParse(item, out databaseId);
                    ExposurePlan ep = exposurePlans.Find(e => e.Id == databaseId);
                    if (ep != null) {
                        OverrideItems.Add(new OverrideItemOld(ep, databaseId));
                    }
                }
            }
        }

        public string Serialize() {
            if (OverrideItems?.Count == 0) {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            foreach (OverrideItemOld item in OverrideItems) {
                sb.Append(item.Serialize()).Append(SEP);
            }

            return sb.ToString().TrimEnd(SEP);
        }

        public ObservableCollection<OverrideItemOld> GetDisplayList() {
            return new ObservableCollection<OverrideItemOld>(OverrideItems);
        }

        public static string Remap(string srcOverrideExposureOrder, List<ExposurePlan> srcExposurePlans, List<ExposurePlan> newExposurePlans) {
            if (string.IsNullOrEmpty(srcOverrideExposureOrder)) { return null; }
            if (srcExposurePlans?.Count != newExposurePlans?.Count) { return null; }

            List<Tuple<int, int>> map = new List<Tuple<int, int>>();
            for (int i = 0; i < srcExposurePlans.Count; i++) {
                map.Add(new Tuple<int, int>(srcExposurePlans[i].Id, newExposurePlans[i].Id));
            }

            OverrideExposureOrderOld overrideExposureOrder = new OverrideExposureOrderOld(srcOverrideExposureOrder, srcExposurePlans);
            StringBuilder sb = new StringBuilder();

            foreach (OverrideItemOld item in overrideExposureOrder.OverrideItems) {
                if (item.IsDither) {
                    sb.Append(DITHER).Append(SEP);
                    continue;
                }

                Tuple<int, int> entry = map.FirstOrDefault(i => i.Item1 == item.ExposurePlanDatabaseId);
                if (entry != null) {
                    sb.Append(entry.Item2).Append(SEP);
                } else {
                    TSLogger.Warning($"failed to find EP ID while remapping pasted exposure plans");
                    return null;
                }
            }

            return sb.ToString().TrimEnd(SEP);
        }
    }

    public class OverrideItemOld {
        public int ExposurePlanDatabaseId { get; private set; }
        public bool IsDither { get; private set; }
        public string Name { get; private set; }

        public OverrideItemOld() {
            IsDither = true;
            ExposurePlanDatabaseId = -1;
            Name = OverrideExposureOrderOld.DITHER;
        }

        public OverrideItemOld(ExposurePlan exposurePlan, int exposurePlanDatabaseId) {
            IsDither = false;
            ExposurePlanDatabaseId = exposurePlanDatabaseId;
            Name = exposurePlan.ExposureTemplate.Name;
        }

        public OverrideItemOld Clone() {
            return new OverrideItemOld {
                IsDither = IsDither,
                ExposurePlanDatabaseId = ExposurePlanDatabaseId,
                Name = Name
            };
        }

        public string Serialize() {
            return IsDither ? OverrideExposureOrderOld.DITHER : ExposurePlanDatabaseId.ToString();
        }
    }
}