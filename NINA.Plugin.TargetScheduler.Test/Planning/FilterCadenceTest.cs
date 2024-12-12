using FluentAssertions;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Planning {

    [TestFixture]
    public class FilterCadenceTest {

        [Test]
        public void testAdvance() {
            new FilterCadence(null).Advance().Should().BeNull();
            List<IFilterCadenceItem> list = new List<IFilterCadenceItem>();
            new FilterCadence(list).Advance().Should().BeNull();

            list.Add(new PlanningFilterCadence(1, true, FilterCadenceAction.Exposure, 0));
            FilterCadence sut = new FilterCadence(list);
            sut.SetLastSelected(list[0]);
            sut.Advance().Should().BeSameAs(list[0]);
            list[0].Next.Should().BeTrue();

            list.Clear();
            list.Add(new PlanningFilterCadence(1, true, FilterCadenceAction.Exposure, 0));
            list.Add(new PlanningFilterCadence(2, false, FilterCadenceAction.Exposure, 1));
            list.Add(new PlanningFilterCadence(3, false, FilterCadenceAction.Exposure, 2));

            sut = new FilterCadence(list);
            sut.Advance().Should().BeNull();

            sut.SetLastSelected(list[1]);
            sut.Advance().Should().BeSameAs(list[2]);
            list[0].Next.Should().BeFalse();
            list[1].Next.Should().BeFalse();
            list[2].Next.Should().BeTrue();

            list.Clear();
            list.Add(new PlanningFilterCadence(1, false, FilterCadenceAction.Exposure, 0));
            list.Add(new PlanningFilterCadence(2, false, FilterCadenceAction.Exposure, 1));
            list.Add(new PlanningFilterCadence(3, true, FilterCadenceAction.Exposure, 2));
            list.Add(new PlanningFilterCadence(4, false, FilterCadenceAction.Exposure, 3));
            list.Add(new PlanningFilterCadence(5, false, FilterCadenceAction.Exposure, 4)); //
            list.Add(new PlanningFilterCadence(6, false, FilterCadenceAction.Exposure, 5));

            sut = new FilterCadence(list);
            sut.SetLastSelected(list[4]);
            sut.Advance().Should().BeSameAs(list[5]);
            list[2].Next.Should().BeFalse();
            list[5].Next.Should().BeTrue();

            sut.SetLastSelected(list[5]);
            sut.Advance().Should().BeSameAs(list[0]);
            list[5].Next.Should().BeFalse();
            list[0].Next.Should().BeTrue();

            /*
            list.Add(new PlanningFilterCadence(1, true, FilterCadenceAction.Exposure, 0));
            IFilterCadenceItem fc = new FilterCadence(list).Advance();
            fc.Should().BeSameAs(list[0]);

            list.Clear();
            list.Add(new PlanningFilterCadence(1, true, FilterCadenceAction.Exposure, 0));
            list.Add(new PlanningFilterCadence(2, false, FilterCadenceAction.Exposure, 1));
            list.Add(new PlanningFilterCadence(3, false, FilterCadenceAction.Exposure, 2));

            fc = new FilterCadence(list).Advance();
            fc.Should().BeSameAs(list[1]);
            list[0].Next.Should().BeFalse();
            list[1].Next.Should().BeTrue();
            list[2].Next.Should().BeFalse();

            fc = new FilterCadence(list).Advance();
            fc.Should().BeSameAs(list[2]);
            list[0].Next.Should().BeFalse();
            list[1].Next.Should().BeFalse();
            list[2].Next.Should().BeTrue();

            fc = new FilterCadence(list).Advance();
            fc.Should().BeSameAs(list[0]);
            list[0].Next.Should().BeTrue();
            list[1].Next.Should().BeFalse();
            list[2].Next.Should().BeFalse();
            */
        }

        [Test]
        public void testAssertProper() {
            FilterCadence sut = new FilterCadence(null);
            List<IFilterCadenceItem> list = new List<IFilterCadenceItem>();
            sut = new FilterCadence(list);

            list.Add(new PlanningFilterCadence(1, false, FilterCadenceAction.Exposure, 0));
            list.Add(new PlanningFilterCadence(2, false, FilterCadenceAction.Exposure, 0));
            list.Add(new PlanningFilterCadence(3, false, FilterCadenceAction.Exposure, 0));
            list.Add(new PlanningFilterCadence(5, false, FilterCadenceAction.Exposure, 0));
            Action create = () => new FilterCadence(list);
            create.Should().Throw<ArgumentException>().WithMessage("incorrect ordering in filter cadence list at index 3");

            list.Clear();
            list.Add(new PlanningFilterCadence(1, false, FilterCadenceAction.Exposure, 0));
            create = () => new FilterCadence(list);
            create.Should().Throw<ArgumentException>().WithMessage("wrong count of next items in filter cadence list: 0");

            list.Add(new PlanningFilterCadence(2, true, FilterCadenceAction.Exposure, 0));
            list.Add(new PlanningFilterCadence(3, true, FilterCadenceAction.Exposure, 1));
            create = () => new FilterCadence(list);
            create.Should().Throw<ArgumentException>().WithMessage("wrong count of next items in filter cadence list: 2");
        }

        [Test]
        public void testEnumerate() {
            FilterCadence sut = new FilterCadence(null);
            sut.GetEnumerator().MoveNext().Should().BeFalse();

            List<IFilterCadenceItem> list = new List<IFilterCadenceItem>();
            sut = new FilterCadence(list);
            sut.GetEnumerator().MoveNext().Should().BeFalse();

            list.Add(new PlanningFilterCadence(1, true, FilterCadenceAction.Exposure, 0));
            sut = new FilterCadence(list);
            int iteration = 0;
            foreach (IFilterCadenceItem item in sut) {
                item.Order = iteration + 1;
                item.Next = iteration == 0;
                item.ReferenceIdx = iteration++;
            }
            iteration.Should().Be(1);

            list.Clear();
            list.Add(new PlanningFilterCadence(1, true, FilterCadenceAction.Exposure, 0));
            list.Add(new PlanningFilterCadence(2, false, FilterCadenceAction.Exposure, 1));

            sut = new FilterCadence(list);
            iteration = 0;
            foreach (IFilterCadenceItem item in sut) {
                item.Order = iteration + 1;
                item.Next = iteration == 0;
                item.ReferenceIdx = iteration++;
            }
            iteration.Should().Be(2);

            list.Clear();
            list.Add(new PlanningFilterCadence(1, false, FilterCadenceAction.Exposure, 2));
            list.Add(new PlanningFilterCadence(2, true, FilterCadenceAction.Exposure, 0));
            list.Add(new PlanningFilterCadence(3, false, FilterCadenceAction.Exposure, 1));

            sut = new FilterCadence(list);
            iteration = 0;
            foreach (IFilterCadenceItem item in sut) {
                item.Next = iteration == 0;
                item.ReferenceIdx = iteration++;
            }
            iteration.Should().Be(3);

            Action reset = () => new FilterCadence(list).GetEnumerator().Reset();
            reset.Should().Throw<NotImplementedException>().WithMessage("not implemented");
        }
    }
}