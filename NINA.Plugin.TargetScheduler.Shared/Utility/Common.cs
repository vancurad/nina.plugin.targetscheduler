using NINA.Core.Utility;

namespace NINA.Plugin.TargetScheduler.Shared.Utility {

    public class Common {
        public static readonly string PLUGIN_HOME = Path.Combine(CoreUtil.APPLICATIONTEMPPATH, "TSPlugin");
        public static readonly bool USE_EMULATOR = false;

        public static long DateTimeToUnixSeconds(DateTime? dateTime) {
            return dateTime == null ? 0 : CoreUtil.DateTimeToUnixTimeStamp((DateTime)dateTime);
        }

        public static DateTime UnixSecondsToDateTime(long? seconds) {
            return CoreUtil.UnixTimeStampToDateTime(seconds == null ? 0 : seconds.Value);
        }

        public static string Base64Encode(string plainText) {
            var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }

        public static string Base64Decode(string encoded) {
            var bytes = Convert.FromBase64String(encoded);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private Common() {
        }
    }
}