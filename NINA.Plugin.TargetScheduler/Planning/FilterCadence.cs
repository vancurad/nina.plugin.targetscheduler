using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Planning {

    public class FilterCadence : IEnumerable {
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
                int nextIdx = idx == filterCadenceList.Count - 1 ? 0 : idx + 1;
                filterCadenceList[idx].Next = false;
                filterCadenceList[nextIdx].Next = true;

                return filterCadenceList[nextIdx];
            }
        }

        public int Count { get => filterCadenceList.Count; }

        public ReadOnlyCollection<IFilterCadenceItem> List { get => filterCadenceList.AsReadOnly(); }

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

        public IEnumerator GetEnumerator() {
            return new FilterCadenceEnumerator(filterCadenceList);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            filterCadenceList.ForEach(fc => { sb.Append(fc).AppendLine(); });
            return sb.ToString();
        }
    }

    internal class FilterCadenceEnumerator : IEnumerator {
        private List<IFilterCadenceItem> filterCadenceList;
        private int currentIdx = -1;
        private int wrappedIdx;
        private bool first = true;

        public FilterCadenceEnumerator(List<IFilterCadenceItem> filterCadenceList) {
            this.filterCadenceList = filterCadenceList;
            if (filterCadenceList.Count > 0) {
                currentIdx = filterCadenceList.FindIndex(fc => fc.Next);
                if (currentIdx == -1) { throw new ArgumentException("filter cadence list has no next=true"); }
                wrappedIdx = currentIdx;
            }
        }

        public object Current => filterCadenceList[currentIdx];

        public bool MoveNext() {
            if (filterCadenceList.Count == 0) return false;

            if (!first) {
                currentIdx = currentIdx == filterCadenceList.Count - 1 ? 0 : currentIdx + 1;
            } else {
                first = false;
                return true;
            }

            return currentIdx != wrappedIdx;
        }

        public void Reset() {
            // should get a fresh IEnumerator
            throw new NotImplementedException("not implemented");
        }
    }
}