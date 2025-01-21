using FluentAssertions;
using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Sequencer;
using NINA.Plugin.TargetScheduler.Test.Astrometry;
using NINA.Plugin.TargetScheduler.Test.Planning;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Sequencer {

    [TestFixture]
    public class PlanExecutionHistoryTest {

        [Test]
        public void testPlanExecutionHistoryItem() {
            DateTime start = DateTime.Now;
            var plan = GetWaitPlan(start);

            PlanExecutionHistoryItem sut = new PlanExecutionHistoryItem(start, plan);
            sut.StartTime.Should().Be(start);
            sut.Plan.Should().Be(plan);

            sut.EndTime = start.AddMinutes(5);
            sut.EndTime.Should().Be(start.AddMinutes(5));
        }

        [Test]
        public void testGetImmediateTargetExposures() {
            var sut = new PlanExecutionHistory();
            var items = sut.GetImmediateTargetExposures();
            items.Item1.Should().BeNull();
            items.Item2.Count.Should().Be(0);

            DateTime start = DateTime.Now;
            var plan = GetWaitPlan(start);
            sut.Add(new PlanExecutionHistoryItem(start, plan));
            items = sut.GetImmediateTargetExposures();
            items.Item1.Should().BeNull();
            items.Item2.Count.Should().Be(0);

            ITarget target = GetTargetPlan(start, "T1", "L").PlanTarget;
            sut.Add(new PlanExecutionHistoryItem(start.AddMinutes(1), GetTargetPlan(start, "T1", "L")));
            sut.Add(new PlanExecutionHistoryItem(start.AddMinutes(1), GetTargetPlan(start, "T1", "R")));
            items = sut.GetImmediateTargetExposures();
            items.Item1.Should().Be(target);
            items.Item2.Count.Should().Be(2);
            items.Item2[0].exposure.FilterName.Should().Be("R");
            items.Item2[1].exposure.FilterName.Should().Be("L");

            // new target so T1 is ignored
            target = GetTargetPlan(start, "T2", "L").PlanTarget;
            sut.Add(new PlanExecutionHistoryItem(start.AddMinutes(1), GetTargetPlan(start, "T2", "G")));
            sut.Add(new PlanExecutionHistoryItem(start.AddMinutes(1), GetTargetPlan(start, "T2", "B")));
            items = sut.GetImmediateTargetExposures();
            items.Item1.Should().Be(target);
            items.Item2.Count.Should().Be(2);
            items.Item2[0].exposure.FilterName.Should().Be("B");
            items.Item2[1].exposure.FilterName.Should().Be("G");
        }

        private SchedulerPlan GetWaitPlan(DateTime planTime) {
            var mockTarget = PlanMocks.GetMockPlanTarget("T1", TestData.M31);
            mockTarget.SetupProperty(t => t.StartTime, planTime.AddMinutes(2));
            return new SchedulerPlan(planTime, null, mockTarget.Object, false);
        }

        private SchedulerPlan GetTargetPlan(DateTime planTime, string name, string filter) {
            var mockExposure = PlanMocks.GetMockPlanExposure(filter, 10, 0);
            mockExposure.SetupProperty(e => e.ExposureLength, 120);

            var target = new PlanningTarget();
            target.StartTime = planTime;
            target.Name = name;
            target.Coordinates = TestData.M31;
            target.SelectedExposure = mockExposure.Object;

            List<IInstruction> list = new List<IInstruction>();
            list.Add(new Plugin.TargetScheduler.Planning.Entities.PlanTakeExposure(mockExposure.Object));

            return new SchedulerPlan(planTime, null, target, list, false);
        }
    }
}