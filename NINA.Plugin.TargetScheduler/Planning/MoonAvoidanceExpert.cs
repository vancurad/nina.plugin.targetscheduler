using NINA.Astrometry;
using NINA.Plugin.TargetScheduler.Astrometry;
using NINA.Plugin.TargetScheduler.Planning.Interfaces;
using NINA.Plugin.TargetScheduler.Shared.Utility;
using NINA.Profile.Interfaces;
using System;

namespace NINA.Plugin.TargetScheduler.Planning {

    /// <summary>
    /// Determine moon avoidance for a target and exposure plan, supporting both classic, relaxed, and absolute (moon above relax max).
    ///
    /// We also calculate an avoidance score that can be used to sort exposures for automatic exposure plan selection.  If a plan was
    /// rejected, the score is zero.  Otherwise the score is:
    /// * 1 if moon down enabled is true
    /// * (required avoidance separation angle) / 180 otherwise
    ///
    /// This will sort exposure plans by maximum aversion to moonlight.  Since any comparison of scores is only for the same target
    /// (and therefore independent of actual target-moon separation), the score can simply use the calculated required avoidance
    /// separation which will always be > 0 and < 180.
    /// </summary>
    public class MoonAvoidanceExpert : IMoonAvoidanceExpert {
        public const double SCORE_OFF = 0;
        public const double SCORE_MAX = 1;

        private ObserverInfo observerInfo;

        public MoonAvoidanceExpert(ObserverInfo observerInfo) {
            this.observerInfo = observerInfo;
        }

        public MoonAvoidanceExpert(IProfile activeProfile) {
            this.observerInfo = new ObserverInfo {
                Latitude = activeProfile.AstrometrySettings.Latitude,
                Longitude = activeProfile.AstrometrySettings.Longitude,
                Elevation = activeProfile.AstrometrySettings.Elevation,
            };
        }

        public bool IsRejected(DateTime atTime, ITarget planTarget, IExposure planExposure) {
            DateTime evaluationTime = atTime;
            double moonAltitude = GetRelaxationMoonAltitude(evaluationTime);

            if (!planExposure.MoonAvoidanceEnabled) {
                planExposure.MoonAvoidanceScore = SCORE_OFF;
                return false;
            }

            double moonSeparationParameter = planExposure.MoonAvoidanceSeparation;
            double moonWidthParameter = planExposure.MoonAvoidanceWidth;

            // If moon altitude is in the relaxation zone, then modulate the separation and width parameters
            if (moonAltitude <= planExposure.MoonRelaxMaxAltitude && planExposure.MoonRelaxScale > 0) {
                moonSeparationParameter = moonSeparationParameter + (planExposure.MoonRelaxScale * (moonAltitude - planExposure.MoonRelaxMaxAltitude));
                moonWidthParameter = moonWidthParameter * ((moonAltitude - planExposure.MoonRelaxMinAltitude) / (planExposure.MoonRelaxMaxAltitude - planExposure.MoonRelaxMinAltitude));
            }

            // Determine avoidance
            double moonAge = GetMoonAge(evaluationTime);
            double moonSeparation = GetMoonSeparationAngle(observerInfo, evaluationTime, planTarget.Coordinates);
            double moonAvoidanceSeparation = AstrometryUtils.GetMoonAvoidanceLorentzianSeparation(moonAge,
                moonSeparationParameter, moonWidthParameter);

            // Avoidance is completely off if the moon is below the relaxation min altitude and relaxation applies
            if (moonAltitude <= planExposure.MoonRelaxMinAltitude && planExposure.MoonRelaxScale > 0) {
                TSLogger.Info($"moon avoidance off: moon altitude ({moonAltitude}) is below relax min altitude ({planExposure.MoonRelaxMinAltitude})");
                planExposure.MoonAvoidanceScore = GetAvoidanceScore(false, planExposure, moonAvoidanceSeparation);
                return false;
            }

            // Avoidance is absolute regardless of moon phase or separation if Moon Must Be Down is enabled
            if (moonAltitude >= planExposure.MoonRelaxMaxAltitude && planExposure.MoonDownEnabled) {
                TSLogger.Info($"moon avoidance absolute: moon altitude ({moonAltitude}) is above relax max altitude ({planExposure.MoonRelaxMaxAltitude}) with Moon Must Be Down enabled");
                planExposure.MoonAvoidanceScore = SCORE_OFF;
                return true;
            }

            // If the separation was relaxed into oblivion, avoidance is off
            if (moonSeparationParameter <= 0) {
                TSLogger.Warning($"moon avoidance separation was relaxed below zero, avoidance off");
                planExposure.MoonAvoidanceScore = GetAvoidanceScore(false, planExposure, moonAvoidanceSeparation);
                return false;
            }

            bool rejected = moonSeparation < moonAvoidanceSeparation;
            planExposure.MoonAvoidanceScore = GetAvoidanceScore(rejected, planExposure, moonAvoidanceSeparation);
            TSLogger.Debug($"moon avoidance {planTarget.Name}/{planExposure.FilterName} rejected={rejected}, eval time={evaluationTime}, moon alt={moonAltitude}, moonSep={moonSeparation}, moonAvoidSep={moonAvoidanceSeparation}");
            return rejected;
        }

        public double GetAvoidanceScore(bool rejected, IExposure planExposure, double moonAvoidanceSeparation) {
            return rejected
                ? SCORE_OFF
                : planExposure.MoonDownEnabled
                    ? SCORE_MAX
                    : moonAvoidanceSeparation / 180;
        }

        public virtual double GetRelaxationMoonAltitude(DateTime evaluationTime) {
            return AstroUtil.GetMoonAltitude(evaluationTime, observerInfo);
        }

        public virtual double GetMoonAge(DateTime atTime) {
            return AstrometryUtils.GetMoonAge(atTime);
        }

        public virtual double GetMoonSeparationAngle(ObserverInfo location, DateTime atTime, Coordinates coordinates) {
            return AstrometryUtils.GetMoonSeparationAngle(observerInfo, atTime, coordinates);
        }
    }

    public interface IMoonAvoidanceExpert {

        bool IsRejected(DateTime atTime, ITarget planTarget, IExposure planExposure);

        double GetRelaxationMoonAltitude(DateTime evaluationTime);

        double GetMoonAge(DateTime atTime);

        double GetMoonSeparationAngle(ObserverInfo location, DateTime atTime, Coordinates coordinates);
    }
}