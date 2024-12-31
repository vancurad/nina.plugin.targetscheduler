using FluentAssertions;
using Moq;
using NINA.Core.Model;
using NINA.Image.ImageData;
using NINA.Image.Interfaces;
using NINA.Plugin.TargetScheduler.Database.Schema;
using NINA.Plugin.TargetScheduler.Grading;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Profile.Interfaces;
using NINA.WPF.Base.Interfaces.Mediator;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NINA.Plugin.TargetScheduler.Test.Grading {

    [TestFixture]
    public class GraderExpertTest {
        private static readonly Guid DefaultProfileId = new Guid("01234567-0000-0000-0000-000000000000");

        [Test]
        public void testNoGradingMetricsEnabled() {
            var prefs = GetPreferences(GetMockProfile(0, 0), 0, 0, false, false, 0, false, 0, false, 0, false, 0, false, 0);
            GraderExpert sut = new GraderExpert(prefs, null);
            sut.NoGradingMetricsEnabled.Should().BeTrue();

            prefs = GetPreferences(GetMockProfile(0, 0), 0, 0, false, false, 0, true, 0, false, 0, false, 0, false, 0);
            sut = new GraderExpert(prefs, null);
            sut.NoGradingMetricsEnabled.Should().BeFalse();
        }

        [Test]
        public void testEnableGradeRMS() {
            var prefs = GetPreferences(GetMockProfile(0, 0), 0, 0, false, true, 0, false, 0, false, 0, false, 0, false, 0);
            GraderExpert sut = new GraderExpert(prefs, null);
            sut.EnableGradeRMS.Should().BeTrue();
            prefs = GetPreferences(GetMockProfile(0, 0), 0, 0, false, false, 0, false, 0, false, 0, false, 0, false, 0);
            sut = new GraderExpert(prefs, null);
            sut.EnableGradeRMS.Should().BeFalse();
        }

        [Test]
        public void testGradeRMS() {
            IProfile profile = GetMockProfile(3.8, 700);
            ImageGraderPreferences prefs = GetPreferences(profile, 0, 0, false, false, 1, false, 0, false, 0, false, 0, false, 0);
            GraderExpert sut = new GraderExpert(prefs, GetMockImageData(0.6, 1, "L", 0, 0));

            sut.GradeRMS().Should().BeTrue(); // not enabled

            prefs = GetPreferences(profile, 0, 0, false, true, 1, false, 0, false, 0, false, 0, false, 0);
            sut = new GraderExpert(prefs, GetMockImageData(0.6, 1, "L", 0, 0));
            sut.GradeRMS().Should().BeTrue();

            sut = new GraderExpert(prefs, GetMockImageData(1, 1, "L", 0, 0));
            sut.GradeRMS().Should().BeFalse();

            prefs = GetPreferences(profile, 0, 0, false, true, 1.35, false, 0, false, 0, false, 0, false, 0);
            sut = new GraderExpert(prefs, GetMockImageData(0.6, 1, "L", 0, 0, 0, 0, 60, 10, 20, "2x2"));
            sut.GradeRMS().Should().BeTrue();

            prefs = GetPreferences(profile, 0, 0, false, true, 1.34, false, 0, false, 0, false, 0, false, 0);
            sut = new GraderExpert(prefs, GetMockImageData(0.6, 1, "L", 0, 0, 0, 0, 60, 10, 20, "2x2"));
            sut.GradeRMS().Should().BeFalse();
        }

        [Test]
        public void testGradeStars() {
            IProfile profile = GetMockProfile(3.8, 700);
            ImageGraderPreferences prefs = GetPreferences(profile, 0, 10, false, false, 0, false, 2, false, 0, false, 0, false, 0);
            List<AcquiredImage> pop = GetTestImages(10, 1, "L", 60);

            GraderExpert sut = new GraderExpert(prefs, GetMockImageData(0, 0, "L", 500, 0));
            sut.GradeStars(pop).Should().BeTrue(); // not enabled

            prefs = GetPreferences(profile, 0, 10, false, false, 0, true, 2, false, 0, false, 0, false, 0);
            sut = new GraderExpert(prefs, GetMockImageData(0, 0, "L", 500, 0));
            sut.GradeStars(pop).Should().BeTrue(); // enabled, within variance

            sut = new GraderExpert(prefs, GetMockImageData(0, 0, "L", 400, 0));
            sut.GradeStars(pop).Should().BeFalse(); // enabled, outside variance

            sut = new GraderExpert(prefs, GetMockImageData(0, 0, "L", 600, 0));
            sut.GradeStars(pop).Should().BeFalse(); // enabled, outside variance, don't accept improvements

            prefs = GetPreferences(profile, 0, 10, true, false, 0, true, 2, false, 0, false, 0, false, 0);
            sut = new GraderExpert(prefs, GetMockImageData(0, 0, "L", 600, 0));
            sut.GradeStars(pop).Should().BeTrue(); // enabled, outside variance, accept improvements
        }

        [Test]
        public void testGradeHFR() {
            IProfile profile = GetMockProfile(3.8, 700);
            ImageGraderPreferences prefs = GetPreferences(profile, 0, 10, false, false, 0, false, 0, false, 0, false, 0, false, 0);
            List<AcquiredImage> pop = GetTestImages(10, 1, "L", 60);

            GraderExpert sut = new GraderExpert(prefs, GetMockImageData(0, 0, "L", 0, 1.5));
            sut.GradeHFR(pop).Should().BeTrue(); // not enabled

            prefs = GetPreferences(profile, 0, 10, false, false, 0, false, 0, true, 2, false, 0, false, 0);
            sut = new GraderExpert(prefs, GetMockImageData(0, 0, "L", 0, 1.5));
            sut.GradeHFR(pop).Should().BeTrue(); // enabled, within variance

            sut = new GraderExpert(prefs, GetMockImageData(0, 0, "L", 0, 3));
            sut.GradeHFR(pop).Should().BeFalse(); // enabled, outside variance

            sut = new GraderExpert(prefs, GetMockImageData(0, 0, "L", 0, 0.2));
            sut.GradeHFR(pop).Should().BeFalse(); // enabled, outside variance, don't accept improvements

            prefs = GetPreferences(profile, 0, 10, true, false, 0, false, 0, true, 2, false, 0, false, 0);
            sut = new GraderExpert(prefs, GetMockImageData(0, 0, "L", 0, 0.2));
            sut.GradeHFR(pop).Should().BeTrue(); // enabled, outside variance, accept improvements
        }

        [Test]
        public void testGradeFWHM() {
            IProfile profile = GetMockProfile(3.8, 700);
            ImageGraderPreferences prefs = GetPreferences(profile, 0, 10, false, false, 0, false, 0, false, 0, false, 0, false, 0);
            ImageSavedEventArgs imageData = GetMockImageData(0, 0, "L", 0, 0, 1.5, 0);
            List<AcquiredImage> pop = GetTestImages(10, 1, "L", 60);

            GraderExpert sut = new GraderExpert(prefs, imageData);
            sut.GradeFWHM(pop).Should().BeTrue(); // not enabled

            prefs = GetPreferences(profile, 0, 10, false, false, 0, false, 0, false, 0, true, 2, false, 0);
            Mock<GraderExpert> mock = new Mock<GraderExpert>(prefs, imageData) { CallBase = true };
            mock.Setup(m => m.GetHocusFocusMetric(imageData.StarDetectionAnalysis, "FWHM")).Returns(2.234);

            sut = mock.Object;
            sut.GradeFWHM(pop).Should().BeTrue(); // enabled, within variance

            mock.Setup(m => m.GetHocusFocusMetric(imageData.StarDetectionAnalysis, "FWHM")).Returns(6);
            sut.GradeFWHM(pop).Should().BeFalse(); // enabled, outside variance

            mock.Setup(m => m.GetHocusFocusMetric(imageData.StarDetectionAnalysis, "FWHM")).Returns(.1);
            sut.GradeFWHM(pop).Should().BeFalse(); // enabled, outside variance, don't accept improvements

            prefs = GetPreferences(profile, 0, 10, true, false, 0, false, 0, false, 0, true, 2, false, 0);
            mock = new Mock<GraderExpert>(prefs, imageData) { CallBase = true };
            mock.Setup(m => m.GetHocusFocusMetric(imageData.StarDetectionAnalysis, "FWHM")).Returns(.1);
            sut = mock.Object;
            sut.GradeFWHM(pop).Should().BeTrue(); // enabled, outside variance, accept improvements
        }

        [Test]
        public void testGradeEccentricity() {
            IProfile profile = GetMockProfile(3.8, 700);
            ImageGraderPreferences prefs = GetPreferences(profile, 0, 10, false, false, 0, false, 0, false, 0, false, 0, false, 0);
            ImageSavedEventArgs imageData = GetMockImageData(0, 0, "L", 0, 0, 1.5, 0);
            List<AcquiredImage> pop = GetTestImages(10, 1, "L", 60);

            GraderExpert sut = new GraderExpert(prefs, imageData);
            sut.GradeEccentricity(pop).Should().BeTrue(); // not enabled

            prefs = GetPreferences(profile, 0, 10, false, false, 0, false, 0, false, 0, false, 0, true, 2);
            Mock<GraderExpert> mock = new Mock<GraderExpert>(prefs, imageData) { CallBase = true };
            mock.Setup(m => m.GetHocusFocusMetric(imageData.StarDetectionAnalysis, "Eccentricity")).Returns(3.456);

            sut = mock.Object;
            sut.GradeEccentricity(pop).Should().BeTrue(); // enabled, within variance

            mock.Setup(m => m.GetHocusFocusMetric(imageData.StarDetectionAnalysis, "Eccentricity")).Returns(10.456);
            sut.GradeEccentricity(pop).Should().BeFalse(); // enabled, outside variance

            mock.Setup(m => m.GetHocusFocusMetric(imageData.StarDetectionAnalysis, "Eccentricity")).Returns(1);
            sut.GradeEccentricity(pop).Should().BeFalse(); // enabled, outside variance, don't accept improvements

            prefs = GetPreferences(profile, 0, 10, true, false, 0, false, 0, false, 0, false, 0, true, 2);
            mock = new Mock<GraderExpert>(prefs, imageData) { CallBase = true };
            mock.Setup(m => m.GetHocusFocusMetric(imageData.StarDetectionAnalysis, "Eccentricity")).Returns(1);
            sut = mock.Object;
            sut.GradeEccentricity(pop).Should().BeTrue(); // enabled, outside variance, accept improvements
        }

        [Test]
        public void testSampleStandardDeviation() {
            var prefs = GetPreferences(GetMockProfile(0, 0), 0, 0, false, false, 0, false, 0, false, 0, false, 0, false, 0);
            GraderExpert sut = new GraderExpert(prefs, null);

            Action act = () => sut.SampleStandardDeviation(null);
            act.Should().Throw<Exception>().Where(e => e.Message == "must have >= 3 samples");

            double[] samples = new double[] { 483, 500 };
            act = () => sut.SampleStandardDeviation(samples.ToList());
            act.Should().Throw<Exception>().Where(e => e.Message == "must have >= 3 samples");

            samples = new double[] { 483, 500, 545 };
            (double mean, double stddev) = sut.SampleStandardDeviation(samples.ToList());
            mean.Should().BeApproximately(509.3333, 0.001);
            stddev.Should().BeApproximately(32.0364, 0.001);
        }

        public static IProfile GetMockProfile(double pixelSize, double focalLength) {
            Mock<IProfileService> mock = new Mock<IProfileService>();
            mock.SetupProperty(m => m.ActiveProfile.Id, DefaultProfileId);
            mock.SetupProperty(m => m.ActiveProfile.CameraSettings.PixelSize, pixelSize);
            mock.SetupProperty(m => m.ActiveProfile.TelescopeSettings.FocalLength, focalLength);
            return mock.Object.ActiveProfile;
        }

        public static ImageGraderPreferences GetPreferences(IProfile profile, double DelayGrading, int SampleSize, bool AcceptImprovement,
                                                      bool GradeRMS, double RMSPixelThreshold,
                                                      bool GradeDetectedStars, double DetectedStarsSigmaFactor,
                                                      bool GradeHFR, double HFRSigmaFactor,
                                                      bool GradeFWHM, double FWHMSigmaFactor,
                                                      bool GradeEccentricity, double EccentricitySigmaFactor) {
            return new ImageGraderPreferences(profile, DelayGrading, SampleSize, AcceptImprovement,
                                              GradeRMS, RMSPixelThreshold,
                                              GradeDetectedStars, DetectedStarsSigmaFactor,
                                              GradeHFR, HFRSigmaFactor,
                                              GradeFWHM, FWHMSigmaFactor, GradeEccentricity, EccentricitySigmaFactor);
        }

        public static ImageSavedEventArgs GetMockImageData(double rmsTotal, double rmsScale, string filter, int detectedStars, double HFR, double FWHM = Double.NaN, double eccentricity = Double.NaN, double duration = 60,
                                               int gain = 10, int offset = 20, string binning = "1x1", double rotation = 0) {
            ImageSavedEventArgs msg = new ImageSavedEventArgs();
            ImageMetaData metadata = new ImageMetaData();
            ImageParameter imageParameter = new ImageParameter();
            CameraParameter cameraParameter = new CameraParameter();

            RMS rms = new RMS { Total = rmsTotal };
            rms.SetScale(rmsScale);

            imageParameter.RecordedRMS = rms;
            imageParameter.Binning = binning;
            metadata.Rotator.Position = rotation;
            metadata.Image = imageParameter;

            cameraParameter.Gain = gain;
            cameraParameter.Offset = offset;
            metadata.Camera = cameraParameter;

            msg.Duration = duration;
            msg.MetaData = metadata;
            msg.Filter = filter;

            Mock<IStarDetectionAnalysis> sdMock = new Mock<IStarDetectionAnalysis>();
            sdMock.SetupProperty(m => m.DetectedStars, detectedStars);
            sdMock.SetupProperty(m => m.HFR, HFR);
            msg.StarDetectionAnalysis = sdMock.Object;

            return msg;
        }

        public static List<AcquiredImage> GetTestImages(int count, int targetId, string filterName,
            double duration = 60, int gain = 10, int offset = 20, string binning = "1x1", double roi = 100,
            double rotatorPosition = 0, GradingStatus status = GradingStatus.Accepted) {
            DateTime dateTime = DateTime.Now.Date;
            List<AcquiredImage> images = new List<AcquiredImage>();
            int id = 501;

            for (int i = 0; i < count; i++) {
                dateTime = dateTime.AddMinutes(5);
                ImageMetadata metaData = new ImageMetadata {
                    ExposureDuration = duration,
                    DetectedStars = 500 + i,
                    HFR = 1 + (double)i / 10,
                    FWHM = 2 + (double)i / 10,
                    Eccentricity = 3 + (double)i / 10,
                    Gain = gain,
                    Offset = offset,
                    Binning = binning,
                    ROI = roi,
                    RotatorPosition = rotatorPosition
                };

                images.Add(new AcquiredImage(DefaultProfileId.ToString(), 0, targetId, 0, dateTime, filterName, status, "", metaData) { Id = id++ });
            }

            return images.OrderByDescending(i => i.AcquiredDate).ToList();
        }

        public static AcquiredImage GetAcquiredImage(int id, int targetId, string filterName,
            double duration = 60, int gain = 10, int offset = 20, string binning = "1x1", double roi = 100,
            double rotatorPosition = 0, GradingStatus status = GradingStatus.Accepted, ImageMetadata metaData = null) {
            return new AcquiredImage(DefaultProfileId.ToString(), 0, targetId, 0, DateTime.Now.Date, filterName, status, "", metaData) { Id = id };
        }
    }
}