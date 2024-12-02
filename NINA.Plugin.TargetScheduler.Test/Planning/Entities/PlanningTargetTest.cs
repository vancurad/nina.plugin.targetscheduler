using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NUnit.Framework;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Planning.Entities {

    [TestFixture]
    public class PlanningTargetTest {

        [Test]
        public void TestPlanningTarget() {
            IProject project = PlanMocks.GetMockPlanProject("project", ProjectState.Active).Object;
            Target target = new Target();
            target.Id = 101;
            target.Name = "target";
            target.ra = 12;
            target.dec = 13;
            target.rotation = 14;
            target.roi = .8;
            target.OverrideExposureOrders = new List<OverrideExposureOrder>();
            target.FilterCadences = new List<FilterCadence>();

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
            sut.OverrideExposureOrders.Should().NotBeNull().And.HaveCount(0);
            sut.FilterCadences.Should().NotBeNull().And.HaveCount(0);

            OverrideExposureOrder oeo = new OverrideExposureOrder();
            oeo.ReferenceIdx = 101;
            target.OverrideExposureOrders.Add(oeo);
            FilterCadence fc = new FilterCadence();
            fc.ReferenceIdx = 202;
            target.FilterCadences.Add(fc);

            sut = new PlanningTarget(project, target);
            sut.OverrideExposureOrders.Should().NotBeNull().And.HaveCount(1);
            sut.OverrideExposureOrders[0].ReferenceIdx.Should().Be(101);
            sut.FilterCadences.Should().NotBeNull().And.HaveCount(1);
            sut.FilterCadences[0].ReferenceIdx.Should().Be(202);
        }
    }
}