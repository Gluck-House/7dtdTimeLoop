using TimeLoop.Models;

namespace TimeLoop.Services {
    public static class ConfigMigrationService {
        public static bool Migrate(ConfigModel config) {
            var changed = false;

            if (config.HordeNightProtection == null) {
                config.HordeNightProtection = new HordeNightProtectionConfig();
                changed = true;
            }

            if (config.MinPlayers < 1) {
                config.MinPlayers = 1;
                changed = true;
            }

            if (config.DaysToSkip < 0) {
                config.DaysToSkip = 0;
                changed = true;
            }

            if (config.LoopLimit < 0) {
                config.LoopLimit = 0;
                changed = true;
            }

            if (config.HordeNightProtection.RewindGraceSeconds < 0) {
                config.HordeNightProtection.RewindGraceSeconds = 0;
                changed = true;
            }

            if (config.ConfigVersion < 1) {
                changed = true;
            }

            if (config.ConfigVersion < 2) {
                changed = true;
            }

            if (config.ConfigVersion != ConfigModel.CurrentVersion) {
                config.ConfigVersion = ConfigModel.CurrentVersion;
                changed = true;
            }

            return changed;
        }
    }
}
