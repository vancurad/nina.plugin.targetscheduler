using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class FilterCadence {
        private object lockObj = new object();
        private List<IFilterCadenceItem> filterCadenceList;

        public FilterCadence(List<IFilterCadenceItem> filterCadenceList) {
            this.filterCadenceList = filterCadenceList is null
                ? new List<IFilterCadenceItem>()
                : filterCadenceList;
            AssertProper();
        }

        public IFilterCadenceItem GetNext() {
            if (filterCadenceList.Count == 0) { return null; }
            return filterCadenceList.Find(fc => fc.Next);
        }

        public IFilterCadenceItem Advance() {
            if (filterCadenceList.Count == 0) { return null; }
            if (filterCadenceList.Count == 1) { return filterCadenceList[0]; }

            lock (lockObj) {
                int idx = filterCadenceList.FindIndex(fc => fc.Next);
                if (idx == -1) { throw new ArgumentException("filter cadence list has no next=true"); }

                // Circular operation
                if (idx == filterCadenceList.Count - 1) {
                    filterCadenceList[idx].Next = false;
                    filterCadenceList[0].Next = true;
                    return filterCadenceList[0];
                } else {
                    filterCadenceList[idx].Next = false;
                    filterCadenceList[idx + 1].Next = true;
                    return filterCadenceList[idx + 1];
                }
            }
        }

        private void AssertProper() {
            if (filterCadenceList.Count == 0) { return; }

            int count = 0;
            int order = 1;
            filterCadenceList.ForEach(fc => {
                if (fc.Next) count++;
                if (fc.Order != order++) {
                    throw new ArgumentException($"incorrect ordering in filter cadence list at index {order - 2}");
                }
            });

            if (count != 1) {
                throw new ArgumentException($"wrong count of next items in filter cadence list: {count}");
            }
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            filterCadenceList.ForEach(fc => { sb.Append(fc).AppendLine(); });
            return sb.ToString();
        }
    }
}