using FluentAssertions;
using Moq;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Planning;
using NINA.Plugin.TargetScheduler.Planning.Entities;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Test.Astrometry;
using NINA.Profile.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NINA.Plugin.TargetScheduler.Test.Planning {

    [TestFixture]
    public class PlannerTest {

        [Test]
        public void testFilterForReadyComplete() {
            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestData.Pittsboro_NC);

            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("M42", TestData.M42);
            Mock<IExposure> pf = PlanMocks.GetMockPlanExposure("Ha", 10, 0);
            PlanMocks.AddMockPlanFilter(pt, pf);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            Mock<IProject> pp2 = PlanMocks.GetMockPlanProject("pp2", ProjectState.Active);
            pt = PlanMocks.GetMockPlanTarget("M31", TestData.M31);
            pf = PlanMocks.GetMockPlanExposure("OIII", 10, 10);
            PlanMocks.AddMockPlanFilter(pt, pf);
            pf = PlanMocks.GetMockPlanExposure("SII", 10, 10);
            PlanMocks.AddMockPlanFilter(pt, pf);
            PlanMocks.AddMockPlanTarget(pp2, pt);

            Assert.That(new Planner(new DateTime(2023, 12, 17, 18, 0, 0), profileMock.Object.ActiveProfile, GetPrefs(), false).FilterForIncomplete(null), Is.Null);

            List<IProject> projects = PlanMocks.ProjectsList(pp1.Object, pp2.Object);
            projects = new Planner(new DateTime(2023, 12, 17, 18, 0, 0), profileMock.Object.ActiveProfile, GetPrefs(), false).FilterForIncomplete(projects);
            Assert.That(projects, Is.Not.Null);
            projects.Count.Should().Be(2);

            IProject pp = projects[0];
            pp.Name.Should().Be("pp1");
            pp.Rejected.Should().BeFalse();
            ITarget pt1 = pp.Targets[0];
            pt1.Rejected.Should().BeFalse();
            IExposure pf1 = pt1.ExposurePlans[0];
            pf1.Rejected.Should().BeFalse();

            pp = projects[1];
            pp.Name.Should().Be("pp2");
            pp.Rejected.Should().BeTrue();
            pp.RejectedReason.Should().Be(Reasons.ProjectComplete);
            pt1 = pp.Targets[0];
            pf1 = pt1.ExposurePlans[0];
            pf1.Rejected.Should().BeTrue();
            pf1.RejectedReason.Should().Be(Reasons.FilterComplete);
            pf1 = pt1.ExposurePlans[1];
            pf1.Rejected.Should().BeTrue();
            pf1.RejectedReason.Should().Be(Reasons.FilterComplete);
        }

        [Test]
        public void testFilterForIncomplete() {
            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestData.Pittsboro_NC);

            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("M42", TestData.M42);
            Mock<IExposure> pf = PlanMocks.GetMockPlanExposure("Ha", 10, 0);
            PlanMocks.AddMockPlanFilter(pt, pf);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            pt = PlanMocks.GetMockPlanTarget("M31", TestData.M31);
            pf = PlanMocks.GetMockPlanExposure("OIII", 10, 10);
            PlanMocks.AddMockPlanFilter(pt, pf);
            pf = PlanMocks.GetMockPlanExposure("SII", 10, 12);
            PlanMocks.AddMockPlanFilter(pt, pf);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            List<IProject> projects = PlanMocks.ProjectsList(pp1.Object);
            projects = new Planner(new DateTime(2023, 12, 17, 18, 0, 0), profileMock.Object.ActiveProfile, GetPrefs(), false).FilterForIncomplete(projects);
            Assert.That(projects, Is.Not.Null);
            projects.Count.Should().Be(1);

            IProject pp = projects[0];
            pp.Name.Should().Be("pp1");
            pp.Rejected.Should().BeFalse();

            ITarget pt1 = pp.Targets[0];
            pt1.Rejected.Should().BeFalse();
            IExposure pf1 = pt1.ExposurePlans[0];
            pf1.Rejected.Should().BeFalse();

            ITarget pt2 = pp.Targets[1];
            pt2.Rejected.Should().BeTrue();
            pt2.RejectedReason.Should().Be(Reasons.TargetComplete);

            IExposure pf2 = pt2.ExposurePlans[0];
            pf2.Rejected.Should().BeTrue();
            pf2.RejectedReason.Should().Be(Reasons.FilterComplete);
            IExposure pf3 = pt2.ExposurePlans[1];
            pf3.Rejected.Should().BeTrue();
            pf3.RejectedReason.Should().Be(Reasons.FilterComplete);
        }

        [Test]
        public void testFilterForIncompleteAllExposuresThrottled() {
            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestData.Pittsboro_NC);
            ProfilePreference prefs = GetPrefs();

            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);

            // Assert setup for throttling
            pp1.Object.EnableGrader.Should().BeFalse();
            prefs.ExposureThrottle.Should().Be(125);

            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("M42", TestData.M42);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            ExposurePlan ep = new ExposurePlan("abcd-1234") { Desired = 12, Acquired = 15 };
            ExposureTemplate et = new ExposureTemplate("abcd-1234", "R", "R");
            IExposure peRed = new PlanningExposure(pt.Object, ep, et);

            ep = new ExposurePlan("abcd-1234") { Desired = 12, Acquired = 15 };
            et = new ExposureTemplate("abcd-1234", "G", "G");
            IExposure peGreen = new PlanningExposure(pt.Object, ep, et);

            ep = new ExposurePlan("abcd-1234") { Desired = 12, Acquired = 10 };
            et = new ExposureTemplate("abcd-1234", "B", "B");
            IExposure peBlue = new PlanningExposure(pt.Object, ep, et);

            pt.Object.ExposurePlans = new List<IExposure>() { peRed, peGreen, peBlue };

            // Blue is not complete ...
            List<IProject> projects = PlanMocks.ProjectsList(pp1.Object);
            projects = new Planner(new DateTime(2023, 12, 15, 18, 0, 0), profileMock.Object.ActiveProfile, prefs, false).FilterForIncomplete(projects);
            projects.Count.Should().Be(1);
            projects[0].Rejected.Should().BeFalse();
            projects[0].Targets[0].Rejected.Should().BeFalse();

            ep = new ExposurePlan("abcd-1234") { Desired = 12, Acquired = 15 };
            et = new ExposureTemplate("abcd-1234", "B", "B");
            peBlue = new PlanningExposure(pt.Object, ep, et);

            pt.Object.ExposurePlans = new List<IExposure>() { peRed, peGreen, peBlue };

            // All are now complete due to throttle
            projects = new Planner(new DateTime(2023, 12, 15, 18, 0, 0), profileMock.Object.ActiveProfile, prefs, false).FilterForIncomplete(projects);
            projects.Count.Should().Be(1);
            projects[0].Rejected.Should().BeTrue();
            projects[0].RejectedReason.Should().Be(Reasons.ProjectComplete);
            projects[0].Targets[0].Rejected.Should().BeTrue();
            projects[0].Targets[0].RejectedReason.Should().Be(Reasons.TargetComplete);
        }

        [Test]
        public void testTargetNoExposurePlans() {
            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestData.Pittsboro_NC);

            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("M42", TestData.M42);
            Mock<IExposure> pf = PlanMocks.GetMockPlanExposure("Ha", 10, 0);
            PlanMocks.AddMockPlanFilter(pt, pf);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            pt = PlanMocks.GetMockPlanTarget("M31", TestData.M31);
            PlanMocks.AddMockPlanTarget(pp1, pt);

            List<IProject> projects = PlanMocks.ProjectsList(pp1.Object);
            projects = new Planner(new DateTime(2023, 12, 17, 18, 0, 0), profileMock.Object.ActiveProfile, GetPrefs(), false).FilterForIncomplete(projects);
            Assert.That(projects, Is.Not.Null);
            projects.Count.Should().Be(1);

            IProject pp = projects[0];
            pp.Name.Should().Be("pp1");
            pp.Rejected.Should().BeFalse();

            ITarget pt1 = pp.Targets[0];
            pt1.Rejected.Should().BeFalse();
            IExposure pf1 = pt1.ExposurePlans[0];
            pf1.Rejected.Should().BeFalse();

            ITarget pt2 = pp.Targets[1];
            pt2.ExposurePlans.Count.Should().Be(0);
            pt2.Rejected.Should().BeTrue();
            pt2.RejectedReason.Should().Be(Reasons.TargetComplete);
        }

        [Test]
        public void testFilterForVisibilityNeverRises() {
            // Southern hemisphere location and IC1805
            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestData.South_Mid_Lat);

            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("IC1805", TestData.IC1805);
            PlanMocks.AddMockPlanTarget(pp1, pt);
            List<IProject> projects = PlanMocks.ProjectsList(pp1.Object);

            projects = new Planner(new DateTime(2023, 12, 17, 18, 0, 0), profileMock.Object.ActiveProfile, GetPrefs(), false).FilterForVisibility(projects);
            Assert.That(projects, Is.Not.Null);
            projects.Count.Should().Be(1);

            IProject pp = projects[0];
            pp.Name.Should().Be("pp1");
            pp.Rejected.Should().BeTrue();
            pp.RejectedReason.Should().Be(Reasons.ProjectAllTargets);
            ITarget pt1 = pp.Targets[0];
            pt1.Rejected.Should().BeTrue();
            pt1.RejectedReason.Should().Be(Reasons.TargetNeverRises);
        }

        [Test]
        public void testFilterForVisibilityNotNow() {
            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestData.Pittsboro_NC);

            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("M42", TestData.M42);
            Mock<IExposure> pf = PlanMocks.GetMockPlanExposure("Ha", 10, 0);
            PlanMocks.AddMockPlanFilter(pt, pf);
            PlanMocks.AddMockPlanTarget(pp1, pt);
            List<IProject> projects = PlanMocks.ProjectsList(pp1.Object);

            projects = new Planner(new DateTime(2023, 6, 17, 18, 0, 0), profileMock.Object.ActiveProfile, GetPrefs(), false).FilterForVisibility(projects);
            Assert.That(projects, Is.Not.Null);
            projects.Count.Should().Be(1);

            IProject pp = projects[0];
            pp.Name.Should().Be("pp1");
            pp.Rejected.Should().BeTrue();
            pp.RejectedReason.Should().Be(Reasons.ProjectAllTargets);
            ITarget pt1 = pp.Targets[0];
            pt1.Rejected.Should().BeTrue();
            pt1.RejectedReason.Should().Be(Reasons.TargetNotVisible);
        }

        [Test]
        public void testFilterForVisibilityVisible() {
            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestData.Pittsboro_NC);

            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("M42", TestData.M42);
            Mock<IExposure> pf = PlanMocks.GetMockPlanExposure("Ha", 10, 0);
            PlanMocks.AddMockPlanFilter(pt, pf);
            PlanMocks.AddMockPlanTarget(pp1, pt);
            List<IProject> projects = PlanMocks.ProjectsList(pp1.Object);

            DateTime atTime = new DateTime(2023, 12, 17, 19, 0, 0);
            projects = new Planner(atTime, profileMock.Object.ActiveProfile, GetPrefs(), false).FilterForVisibility(projects);
            Assert.That(projects, Is.Not.Null);
            projects.Count.Should().Be(1);

            IProject pp = projects[0];
            pp.Name.Should().Be("pp1");
            pp.Rejected.Should().BeFalse();
            ITarget pt1 = pp.Targets[0];
            pt1.Rejected.Should().BeFalse();

            TimeSpan precision = TimeSpan.FromSeconds(1);
            pt1.StartTime.Should().BeCloseTo(new DateTime(2023, 12, 17, 18, 59, 54), precision);
            pt1.CulminationTime.Should().BeCloseTo(new DateTime(2023, 12, 18, 0, 5, 45), precision);
            pt1.EndTime.Should().BeCloseTo(new DateTime(2023, 12, 18, 5, 47, 59), precision);
        }

        [Test]
        public void testFilterForVisibilityInMeridianWindow() {
            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestData.Pittsboro_NC);

            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            pp1.SetupProperty(m => m.MeridianWindow, 30);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("M42", TestData.M42);
            Mock<IExposure> pf = PlanMocks.GetMockPlanExposure("Ha", 10, 0);
            PlanMocks.AddMockPlanFilter(pt, pf);
            PlanMocks.AddMockPlanTarget(pp1, pt);
            List<IProject> projects = PlanMocks.ProjectsList(pp1.Object);

            projects = new Planner(new DateTime(2023, 12, 17, 23, 36, 0), profileMock.Object.ActiveProfile, GetPrefs(), false).FilterForVisibility(projects);
            Assert.That(projects, Is.Not.Null);
            projects.Count.Should().Be(1);

            IProject pp = projects[0];
            pp.Name.Should().Be("pp1");
            pp.Rejected.Should().BeFalse();
            ITarget pt1 = pp.Targets[0];
            pt1.Rejected.Should().BeFalse();

            TimeSpan precision = TimeSpan.FromSeconds(1);
            pt1.StartTime.Should().BeCloseTo(new DateTime(2023, 12, 17, 23, 35, 54), precision);
            pt1.CulminationTime.Should().BeCloseTo(new DateTime(2023, 12, 18, 0, 5, 45), precision);
            pt1.EndTime.Should().BeCloseTo(new DateTime(2023, 12, 18, 0, 35, 45), precision);
        }

        [Test]
        public void testFilterForVisibilityWaitForMeridianWindow() {
            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestData.Pittsboro_NC);

            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            pp1.SetupProperty(m => m.MeridianWindow, 30);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("M42", TestData.M42);
            Mock<IExposure> pf = PlanMocks.GetMockPlanExposure("Ha", 10, 0);
            PlanMocks.AddMockPlanFilter(pt, pf);
            PlanMocks.AddMockPlanTarget(pp1, pt);
            List<IProject> projects = PlanMocks.ProjectsList(pp1.Object);

            projects = new Planner(new DateTime(2023, 12, 17, 19, 0, 0), profileMock.Object.ActiveProfile, GetPrefs(), false).FilterForVisibility(projects);
            Assert.That(projects, Is.Not.Null);
            projects.Count.Should().Be(1);

            IProject pp = projects[0];
            pp.Name.Should().Be("pp1");
            pp.Rejected.Should().BeTrue();
            ITarget pt1 = pp.Targets[0];
            pt1.Rejected.Should().BeTrue();
            pt1.RejectedReason.Should().Be(Reasons.TargetBeforeMeridianWindow);
        }

        [Test]
        public void testFilterForVisibilityMeridianWindowCircumpolar() {
            Mock<IProfileService> profileMock = PlanMocks.GetMockProfileService(TestData.Sanikiluaq_NU);

            Mock<IProject> pp1 = PlanMocks.GetMockPlanProject("pp1", ProjectState.Active);
            pp1.SetupProperty(m => m.MeridianWindow, 30);
            Mock<ITarget> pt = PlanMocks.GetMockPlanTarget("IC1805", TestData.IC1805);
            Mock<IExposure> pf = PlanMocks.GetMockPlanExposure("Ha", 10, 0);
            PlanMocks.AddMockPlanFilter(pt, pf);
            PlanMocks.AddMockPlanTarget(pp1, pt);
            List<IProject> projects = PlanMocks.ProjectsList(pp1.Object);

            projects = new Planner(new DateTime(2023, 12, 17, 20, 38, 0), profileMock.Object.ActiveProfile, GetPrefs(), false).FilterForVisibility(projects);
            Assert.That(projects, Is.Not.Null);
            projects.Count.Should().Be(1);

            IProject pp = projects[0];
            pp.Name.Should().Be("pp1");
            pp.Rejected.Should().BeFalse();
            ITarget pt1 = pp.Targets[0];
            pt1.Rejected.Should().BeFalse();

            TimeSpan precision = TimeSpan.FromSeconds(1);
            pt1.StartTime.Should().BeCloseTo(new DateTime(2023, 12, 17, 20, 37, 55), precision);
            pt1.CulminationTime.Should().BeCloseTo(new DateTime(2023, 12, 17, 21, 7, 18), precision);
            pt1.EndTime.Should().BeCloseTo(new DateTime(2023, 12, 17, 21, 37, 18), precision);
        }

        private ProfilePreference GetPrefs(string profileId = "abcd-1234") {
            return new ProfilePreference(profileId);
        }
    }
}