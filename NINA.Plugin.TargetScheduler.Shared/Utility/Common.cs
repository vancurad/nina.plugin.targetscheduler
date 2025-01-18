using NINA.Core.Utility;
using System.Collections;

namespace NINA.Plugin.TargetScheduler.Shared.Utility {

    public class Common {
        public static readonly string PLUGIN_HOME = Path.Combine(CoreUtil.APPLICATIONTEMPPATH, "SchedulerPlugin");

        public static long DateTimeToUnixSeconds(DateTime? dateTime) {
            return dateTime == null ? 0 : CoreUtil.DateTimeToUnixTimeStamp((DateTime)dateTime);
        }

        public static DateTime UnixSecondsToDateTime(long? seconds) {
            return CoreUtil.UnixTimeStampToDateTime(seconds == null ? 0 : seconds.Value);
        }

        public static bool IsEmpty(IList list) {
            return list is null or [];
        }

        public static bool IsNotEmpty(IList list) {
            return !IsEmpty(list);
        }

        private Common() {
        }
    }
}