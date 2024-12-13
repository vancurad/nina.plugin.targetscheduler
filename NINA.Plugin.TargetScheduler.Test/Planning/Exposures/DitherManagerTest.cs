using FluentAssertions;
using NINA.Plugin.TargetScheduler.Planning.Exposures;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NUnit.Framework;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Exposures {

    [TestFixture]
    public class DitherManagerTest {

        [Test]
        public void testDitherEveryZero() {
            DitherManager sut = new DitherManager(0);

            IExposure L = GetExposure("L");
            IExposure R = GetExposure("R");
            IExposure G = GetExposure("G");
            IExposure B = GetExposure("B");

            sut.AddExposure(L);
            sut.AddExposure(R);
            sut.AddExposure(G);
            sut.AddExposure(B);
            sut.AddExposure(L);
            sut.AddExposure(R);
            sut.AddExposure(G);
            sut.AddExposure(B);
            sut.AddExposure(L);
            sut.AddExposure(R);
            sut.AddExposure(G);
            sut.AddExposure(B);

            sut.DitherRequired(L).Should().BeFalse();
            sut.DitherRequired(R).Should().BeFalse();
            sut.DitherRequired(G).Should().BeFalse();
            sut.DitherRequired(B).Should().BeFalse();
        }

        [Test]
        public void testDitherEveryOne() {
            DitherManager sut = new DitherManager(1);

            IExposure L = GetExposure("L");
            IExposure R = GetExposure("R");
            IExposure G = GetExposure("G");
            IExposure B = GetExposure("B");

            sut.DitherRequired(L).Should().BeFalse();
            sut.AddExposure(L);
            sut.DitherRequired(L).Should().BeTrue();
            sut.Reset();

            sut.DitherRequired(L).Should().BeFalse();
            sut.AddExposure(L);
            sut.DitherRequired(R).Should().BeFalse();
            sut.AddExposure(R);
            sut.DitherRequired(G).Should().BeFalse();
            sut.AddExposure(G);
            sut.DitherRequired(B).Should().BeFalse();
            sut.AddExposure(B);

            sut.DitherRequired(L).Should().BeTrue();
            sut.Reset();
            sut.AddExposure(L);
            sut.DitherRequired(R).Should().BeFalse();
        }

        [Test]
        public void testDitherEveryTwo() {
            DitherManager sut = new DitherManager(2);

            IExposure L = GetExposure("L");
            IExposure R = GetExposure("R");
            IExposure G = GetExposure("G");
            IExposure B = GetExposure("B");

            sut.DitherRequired(L).Should().BeFalse();
            sut.AddExposure(L);
            sut.DitherRequired(L).Should().BeFalse();
            sut.AddExposure(L);
            sut.DitherRequired(L).Should().BeTrue();
            sut.Reset();
        }

        private IExposure GetExposure(string filterName) {
            return PlanMocks.GetMockPlanExposure(filterName, 1, 0).Object;
        }
    }
}