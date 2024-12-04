using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;

namespace NINA.Plugin.TargetScheduler.Test.Planning {

    [TestFixture]
    public class DitherInjectorTest {

        [Test]
        [TestCase("LRGB", "LRGB", 0)]
        [TestCase("LR", "LRd", 1)]
        [TestCase("LLRR", "LdLRdR", 1)]
        [TestCase("LLRRL", "LdLRdRLd", 1)]
        [TestCase("LLRRGGBB", "LdLRdRGdGBdB", 1)]
        [TestCase("LRGBLRGBLRGBLLL", "LRGBdLRGBdLRGBdLdLdLd", 1)]
        [TestCase("LRGBLRGBLRGBLLL", "LRGBLRGBdLRGBLdLLd", 2)]
        [TestCase("LRGBLRGBLRGBLLL", "LRGBLRGBLRGBdLLLd", 3)]
        [TestCase("LLRRGGBBLL", "LdLRdRGdGBdBLdLd", 1)]
        [TestCase("LLRRGGBBLL", "LLRRGGBBdLLd", 2)]
        [TestCase("LLLRRRGGGBBB", "LLdLRRdRGGdGBBdB", 2)]
        [TestCase("LLLRRRGGGBBBLLLRRRGGGBBBLLLRRRGGGBBBL", "LLLRRRGGGBBBdLLLRRRGGGBBBdLLLRRRGGGBBBdL", 3)]
        [TestCase("HHOOHHOO", "HHOOdHHOOd", 2)]
        [TestCase("HHHH", "HHdHHd", 2)]
        [TestCase("H", "Hd", 1)]
        public void testFromFilterCadences(string seq, string expected, int ditherEvery) {
            (List<IExposure> eps, List<IFilterCadenceItem> fcs) = GetTestLists(seq);
            var dithered = new DitherInjector(fcs, eps, ditherEvery).Inject();
            GetCompare(dithered, eps).Should().Be(expected);
        }

        private (List<IExposure>, List<IFilterCadenceItem>) GetTestLists(string seq) {
            List<IExposure> eps = new List<IExposure>();
            List<IFilterCadenceItem> fcs = new List<IFilterCadenceItem>();

            char[] chars = seq.ToCharArray();
            foreach (char c in chars) {
                string filterName = c.ToString();
                if (eps.Find(ep => ep.FilterName == filterName) == null) {
                    eps.Add(GetMockExposure(filterName));
                }
            }

            int order = 1;
            chars = seq.ToCharArray();
            foreach (char c in chars) {
                string filterName = c.ToString();
                int idx = eps.FindIndex(ep => ep.FilterName == filterName);
                fcs.Add(GetMockFilterCadence(order++, false, FilterCadenceAction.Exposure, idx));
            }

            return (eps, fcs);
        }

        private string GetCompare(List<IFilterCadenceItem> fcs, List<IExposure> eps) {
            StringBuilder dithered = new StringBuilder();
            fcs.ForEach(fc => {
                string s = fc.Action == FilterCadenceAction.Exposure
                ? eps[fc.ReferenceIdx].FilterName
                : "d";
                dithered.Append(s);
            });

            return dithered.ToString();
        }

        private IExposure GetMockExposure(string filterName) {
            Mock<IExposure> e = new Mock<IExposure>();
            e.SetupAllProperties();
            e.SetupProperty(e => e.FilterName, filterName);
            return e.Object;
        }

        private IFilterCadenceItem GetMockFilterCadence(int order, bool next, FilterCadenceAction action, int refIdx) {
            Mock<IFilterCadenceItem> f = new Mock<IFilterCadenceItem>();
            f.SetupAllProperties();
            f.SetupProperty(f => f.Order, order);
            f.SetupProperty(f => f.Next, next);
            f.SetupProperty(f => f.Action, action);
            f.SetupProperty(f => f.ReferenceIdx, refIdx);
            return f.Object;
        }
    }
}