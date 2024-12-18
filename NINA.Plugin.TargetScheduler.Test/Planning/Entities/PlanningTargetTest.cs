using FluentAssertions;
using NINA.Core.Model.Equipment;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Exposures;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NUnit.Framework;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Entities {

    [TestFixture]
    public class PlanningTargetTest {

        [Test]
        public void TestPlanningTarget() {
            IProject project = PlanMocks.GetMockPlanProject("project", ProjectState.Active).Object;
            project.FilterSwitchFrequency = 1;
            Target target = new Target();
            target.Id = 101;
            target.Name = "target";
            target.ra = 12;
            target.dec = 13;
            target.rotation = 14;
            target.roi = .8;
            target.OverrideExposureOrders = new List<OverrideExposureOrderItem>();
            target.FilterCadences = new List<FilterCadenceItem>();

            PlanningTarget sut = new PlanningTarget(project, target);
            sut.PlanId.Should().NotBeNull();
            sut.DatabaseId = 101;
            sut.Name = "target";
            sut.Coordinates.Should().Be(target.Coordinates);
            sut.Rotation.Should().Be(14);
            sut.ROI.Should().Be(.8);
            sut.Rejected.Should().BeFalse();
            sut.ExposurePlans.Should().NotBeNull().And.HaveCount(0);
            sut.CompletedExposurePlans.Should().NotBeNull().And.HaveCount(0);
            sut.ExposureSelector.Should().NotBeNull();
            (sut.ExposureSelector is BasicExposureSelector).Should().BeTrue();
        }

        [Test]
        public void TestWithOverrideExposureOrder() {
            IProject project = PlanMocks.GetMockPlanProject("project", ProjectState.Active).Object;
            Target target = new Target();
            target.Id = 101;
            target.Name = "target";
            target.ra = 12;
            target.dec = 13;
            target.rotation = 14;
            target.roi = .8;
            target.FilterCadences = new List<FilterCadenceItem>();

            ExposurePlan L = GetEP("L");
            ExposurePlan R = GetEP("R");
            ExposurePlan G = GetEP("G");
            ExposurePlan B = GetEP("B");
            target.ExposurePlans = new List<ExposurePlan>();
            target.ExposurePlans.Add(L);
            target.ExposurePlans.Add(R);
            target.ExposurePlans.Add(G);
            target.ExposurePlans.Add(B);

            target.OverrideExposureOrders = new List<OverrideExposureOrderItem>();
            target.OverrideExposureOrders.Add(GetOEO(1, 1, OverrideExposureOrderAction.Exposure, 0));
            target.OverrideExposureOrders.Add(GetOEO(1, 2, OverrideExposureOrderAction.Exposure, 0));
            target.OverrideExposureOrders.Add(GetOEO(1, 3, OverrideExposureOrderAction.Dither, -1));
            target.OverrideExposureOrders.Add(GetOEO(1, 4, OverrideExposureOrderAction.Exposure, 1));
            target.OverrideExposureOrders.Add(GetOEO(1, 5, OverrideExposureOrderAction.Exposure, 1));
            target.OverrideExposureOrders.Add(GetOEO(1, 6, OverrideExposureOrderAction.Dither, -1));

            PlanningTarget sut = new PlanningTarget(project, target);
            sut.ExposurePlans.Count.Should().Be(2);
            sut.ExposurePlans[0].FilterName.Should().Be("L");
            sut.ExposurePlans[1].FilterName.Should().Be("R");

            sut.ExposureSelector.Should().NotBeNull();
            (sut.ExposureSelector is OverrideOrderExposureSelector).Should().BeTrue();
        }

        private ExposurePlan GetEP(string filterName) {
            var ep = new ExposurePlan();
            ep.ExposureTemplate = new ExposureTemplate { FilterName = filterName, BinningMode = new BinningMode(1, 1) };
            return ep;
        }

        private OverrideExposureOrderItem GetOEO(int tid, int order, OverrideExposureOrderAction action, int refIdx) {
            return new OverrideExposureOrderItem { TargetId = tid, Order = order, Action = action, ReferenceIdx = refIdx };
        }
    }
}