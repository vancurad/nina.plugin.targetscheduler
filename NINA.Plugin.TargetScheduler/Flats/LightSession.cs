using NINA.Plugin.TargetScheduler.Util;
using System;

namespace NINA.Plugin.TargetScheduler.Flats {

    public class LightSession : IComparable, IEquatable<LightSession> {
        public int TargetId { get; private set; }
        public DateTime SessionDate { get; private set; }
        public int SessionId { get; private set; }
        public FlatSpec FlatSpec { get; private set; }

        public LightSession(int targetId, DateTime sessionDate, int sessionId, FlatSpec flatSpec) {
            TargetId = targetId;
            SessionDate = sessionDate;
            SessionId = sessionId;
            FlatSpec = flatSpec;
        }

        public override string ToString() {
            return $"{TargetId} {Utils.FormatDateTimeFull(SessionDate)} {SessionId} {FlatSpec}";
        }

        public int CompareTo(object obj) {
            LightSession lightSession = obj as LightSession;
            return (lightSession != null) ? SessionDate.CompareTo(lightSession.SessionDate) : 0;
        }

        public bool Equals(LightSession other) {
            if (other is null) { return false; }
            if (ReferenceEquals(this, other)) { return true; }
            if (GetType() != other.GetType()) { return false; }

            return TargetId == other.TargetId
                && SessionDate == other.SessionDate
                && SessionId == other.SessionId
                && FlatSpec.Equals(other.FlatSpec);
        }
    }
}