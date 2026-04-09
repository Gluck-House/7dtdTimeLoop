using TimeLoop.Enums;

namespace TimeLoop.Models {
    public readonly struct TimeLoopStatus {
        public TimeLoopStatus(
            bool isTimeFlowing,
            EMode mode,
            PlayerActivitySummary playerActivity,
            int daysToSkip,
            int loopLimit,
            int timesLooped,
            bool isHordeRewindPending,
            int pendingHordeRewindSeconds,
            BloodMoonStatus bloodMoonStatus) {
            IsTimeFlowing = isTimeFlowing;
            Mode = mode;
            PlayerActivity = playerActivity;
            DaysToSkip = daysToSkip;
            LoopLimit = loopLimit;
            TimesLooped = timesLooped;
            IsHordeRewindPending = isHordeRewindPending;
            PendingHordeRewindSeconds = pendingHordeRewindSeconds;
            BloodMoonStatus = bloodMoonStatus;
        }

        public bool IsTimeFlowing { get; }

        public EMode Mode { get; }

        public PlayerActivitySummary PlayerActivity { get; }

        public int DaysToSkip { get; }

        public int LoopLimit { get; }

        public int TimesLooped { get; }

        public bool IsHordeRewindPending { get; }

        public int PendingHordeRewindSeconds { get; }

        public BloodMoonStatus BloodMoonStatus { get; }
    }
}
