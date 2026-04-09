using TimeLoop.Enums;
using TimeLoop.Models;

namespace TimeLoop.Services {
    public static class TimeLoopPolicy {
        public static bool DetermineTimeFlowing(ConfigModel config, PlayerActivitySummary players) {
            if (!config.Enabled)
                return true;

            return config.Mode switch {
                EMode.Whitelist => players.AuthorizedPlayers > 0,
                EMode.Threshold => players.ConnectedPlayers >= config.MinPlayers,
                EMode.WhitelistedThreshold => players.AuthorizedPlayers >= config.MinPlayers,
                EMode.Always => false,
                _ => false
            };
        }

        public static bool IsLoopLimitReached(int timesLooped, int loopLimit) {
            return loopLimit > 0 && timesLooped >= loopLimit;
        }

        public static bool ShouldSkipCurrentLoop(bool isTimeFlowing, int daysToSkip) {
            return !isTimeFlowing && daysToSkip > 0;
        }

        public static bool CanScheduleHordeNightRewind(ConfigModel config, BloodMoonStatus bloodMoonStatus) {
            return config.HordeNightProtection.Enabled
                   && bloodMoonStatus.IsScheduledBloodMoonDay
                   && bloodMoonStatus.IsBeforeBloodMoonStart;
        }
    }
}
