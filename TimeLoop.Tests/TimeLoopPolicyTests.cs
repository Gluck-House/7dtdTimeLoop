using TimeLoop.Enums;
using TimeLoop.Models;
using TimeLoop.Services;

namespace TimeLoop.Tests;

public class TimeLoopPolicyTests {
    [Fact]
    public void DetermineTimeFlowing_RespectsDisabledConfig() {
        var config = new ConfigModel { Enabled = false, Mode = EMode.Always };

        var result = TimeLoopPolicy.DetermineTimeFlowing(config, new PlayerActivitySummary(0, 0));

        Assert.True(result);
    }

    [Fact]
    public void DetermineTimeFlowing_WhitelistMode_RequiresAuthorizedPlayer() {
        var config = new ConfigModel { Enabled = true, Mode = EMode.Whitelist };

        Assert.False(TimeLoopPolicy.DetermineTimeFlowing(config, new PlayerActivitySummary(3, 0)));
        Assert.True(TimeLoopPolicy.DetermineTimeFlowing(config, new PlayerActivitySummary(3, 1)));
    }

    [Fact]
    public void DetermineTimeFlowing_ThresholdMode_UsesConnectedPlayerCount() {
        var config = new ConfigModel { Enabled = true, Mode = EMode.Threshold, MinPlayers = 2 };

        Assert.False(TimeLoopPolicy.DetermineTimeFlowing(config, new PlayerActivitySummary(1, 1)));
        Assert.True(TimeLoopPolicy.DetermineTimeFlowing(config, new PlayerActivitySummary(2, 0)));
    }

    [Fact]
    public void DetermineTimeFlowing_WhitelistedThresholdMode_UsesAuthorizedPlayerCount() {
        var config = new ConfigModel { Enabled = true, Mode = EMode.WhitelistedThreshold, MinPlayers = 2 };

        Assert.False(TimeLoopPolicy.DetermineTimeFlowing(config, new PlayerActivitySummary(4, 1)));
        Assert.True(TimeLoopPolicy.DetermineTimeFlowing(config, new PlayerActivitySummary(4, 2)));
    }

    [Fact]
    public void CanScheduleHordeNightRewind_RequiresEnabledScheduledBloodMoonBeforeStart() {
        var config = new ConfigModel {
            HordeNightProtection = new HordeNightProtectionConfig(true, 300)
        };

        Assert.True(TimeLoopPolicy.CanScheduleHordeNightRewind(config, new BloodMoonStatus(7, 21000, 22000, true)));
        Assert.False(TimeLoopPolicy.CanScheduleHordeNightRewind(config, new BloodMoonStatus(7, 23000, 22000, true)));
        Assert.False(TimeLoopPolicy.CanScheduleHordeNightRewind(config, new BloodMoonStatus(6, 21000, 22000, false)));

        config.HordeNightProtection.Enabled = false;
        Assert.False(TimeLoopPolicy.CanScheduleHordeNightRewind(config, new BloodMoonStatus(7, 21000, 22000, true)));
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(0, 1, false)]
    [InlineData(1, 1, true)]
    [InlineData(2, 1, true)]
    public void IsLoopLimitReached_WorksAsExpected(int timesLooped, int loopLimit, bool expected) {
        Assert.Equal(expected, TimeLoopPolicy.IsLoopLimitReached(timesLooped, loopLimit));
    }

    [Theory]
    [InlineData(false, 1, true)]
    [InlineData(false, 0, false)]
    [InlineData(true, 1, false)]
    public void ShouldSkipCurrentLoop_WorksAsExpected(bool isTimeFlowing, int daysToSkip, bool expected) {
        Assert.Equal(expected, TimeLoopPolicy.ShouldSkipCurrentLoop(isTimeFlowing, daysToSkip));
    }
}
