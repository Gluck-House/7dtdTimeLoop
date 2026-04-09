using TimeLoop.Enums;
using TimeLoop.Models;
using TimeLoop.Services;

namespace TimeLoop.Tests;

public class ConfigMigrationServiceTests {
    [Fact]
    public void Migrate_UpgradesLegacyConfigAndNormalizesValues() {
        var config = new ConfigModel {
            ConfigVersion = 0,
            Enabled = true,
            Mode = EMode.Whitelist,
            MinPlayers = 0,
            DaysToSkip = -5,
            LoopLimit = -2,
            Language = "",
            HordeNightProtection = null!
        };

        var changed = ConfigMigrationService.Migrate(config);

        Assert.True(changed);
        Assert.Equal(ConfigModel.CurrentVersion, config.ConfigVersion);
        Assert.Equal(1, config.MinPlayers);
        Assert.Equal(0, config.DaysToSkip);
        Assert.Equal(0, config.LoopLimit);
        Assert.Equal("en_us", config.Language);
        Assert.NotNull(config.HordeNightProtection);
        Assert.Equal(300, config.HordeNightProtection.RewindGraceSeconds);
    }

    [Fact]
    public void LegacyProxyProperties_WriteIntoGroupedHordeNightProtection() {
        var config = new ConfigModel();

        config.LegacyProtectHordeNights = false;
        config.LegacyHordeRewindGraceSeconds = 42;

        Assert.False(config.HordeNightProtection.Enabled);
        Assert.Equal(42, config.HordeNightProtection.RewindGraceSeconds);
    }

    [Fact]
    public void Migrate_ClampsNegativeHordeGraceForCurrentSchema() {
        var config = new ConfigModel {
            ConfigVersion = ConfigModel.CurrentVersion,
            HordeNightProtection = new HordeNightProtectionConfig(true, -15)
        };

        var changed = ConfigMigrationService.Migrate(config);

        Assert.True(changed);
        Assert.Equal(0, config.HordeNightProtection.RewindGraceSeconds);
    }
}
