using NINA.Astrometry;

namespace NINA.Plugin.TargetScheduler.Astrometry {

    public enum TwilightLevel {
        Nighttime, Astronomical, Nautical, Civil
    };

    public enum TwilightStage {

        // TODO: better way to deal with this?
        Dusk, Dawn
    };

    public static class Twilight {

        // TODO: these were originally privates on NighttimeCircumstances
        public static double SunSetRiseAltitude { get => AstroUtil.ArcminToDegree(-50); } // refraction adjustment

        public static double CivilSunAltitude { get => -6; }
        public static double NauticalSunAltitude { get => -12; }
        public static double AstronomicalSunAltitude { get => -18; }
    }
}