using System.Xml.Linq;
using TimeLoop.Enums;
using TimeLoop.Models;
using TimeLoop.Wrappers;

namespace TimeLoop.Tests;

public class XmlSerializerWrapperTests {
    [Fact]
    public void ToXmlAndFromXml_RoundTripConfigModel() {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        try {
            var original = new ConfigModel {
                ConfigVersion = ConfigModel.CurrentVersion,
                Enabled = true,
                Mode = EMode.WhitelistedThreshold,
                MinPlayers = 3,
                DaysToSkip = 2,
                LoopLimit = 4,
                HordeNightProtection = new HordeNightProtectionConfig(false, 90),
                Players = new List<PlayerModel> {
                    new() { Id = "steam_1", PlayerName = "Alice", IsAuthorized = true },
                    new() { Id = "steam_2", PlayerName = "Bob", IsAuthorized = false }
                }
            };

            XmlSerializerWrapper.ToXml(path, original);
            var roundTripped = XmlSerializerWrapper.FromXml<ConfigModel>(path);

            Assert.Equal(original.ConfigVersion, roundTripped.ConfigVersion);
            Assert.Equal(original.Enabled, roundTripped.Enabled);
            Assert.Equal(original.Mode, roundTripped.Mode);
            Assert.Equal(original.MinPlayers, roundTripped.MinPlayers);
            Assert.Equal(original.DaysToSkip, roundTripped.DaysToSkip);
            Assert.Equal(original.LoopLimit, roundTripped.LoopLimit);
            Assert.Equal(original.HordeNightProtection.Enabled, roundTripped.HordeNightProtection.Enabled);
            Assert.Equal(original.HordeNightProtection.RewindGraceSeconds,
                roundTripped.HordeNightProtection.RewindGraceSeconds);
            Assert.Collection(roundTripped.Players,
                player => {
                    Assert.Equal("steam_1", player.Id);
                    Assert.Equal("Alice", player.PlayerName);
                    Assert.True(player.IsAuthorized);
                },
                player => {
                    Assert.Equal("steam_2", player.Id);
                    Assert.Equal("Bob", player.PlayerName);
                    Assert.False(player.IsAuthorized);
                });
        }
        finally {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void FromXmlOverwrite_ReplacesExistingPublicMembers() {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        try {
            var source = new ConfigModel {
                Enabled = false,
                Mode = EMode.Threshold,
                MinPlayers = 7,
                DaysToSkip = 1,
                LoopLimit = 9,
                HordeNightProtection = new HordeNightProtectionConfig(false, 45),
                Players = new List<PlayerModel> {
                    new() { Id = "steam_9", PlayerName = "Carol", IsAuthorized = true }
                }
            };
            XmlSerializerWrapper.ToXml(path, source);

            var destination = new ConfigModel {
                Enabled = true,
                Mode = EMode.Always,
                MinPlayers = 1,
                DaysToSkip = 0,
                LoopLimit = 0,
                HordeNightProtection = new HordeNightProtectionConfig(true, 300),
                Players = new List<PlayerModel>()
            };

            XmlSerializerWrapper.FromXmlOverwrite(path, destination);

            Assert.Equal(source.Enabled, destination.Enabled);
            Assert.Equal(source.Mode, destination.Mode);
            Assert.Equal(source.MinPlayers, destination.MinPlayers);
            Assert.Equal(source.DaysToSkip, destination.DaysToSkip);
            Assert.Equal(source.LoopLimit, destination.LoopLimit);
            Assert.Equal(source.HordeNightProtection.Enabled, destination.HordeNightProtection.Enabled);
            Assert.Equal(source.HordeNightProtection.RewindGraceSeconds,
                destination.HordeNightProtection.RewindGraceSeconds);
            Assert.Single(destination.Players);
            Assert.Equal("Carol", destination.Players[0].PlayerName);
        }
        finally {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void HasMissingSerializedMembers_IgnoresLegacyProxyFieldsForCurrentSchema() {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        try {
            var config = new ConfigModel();

            XmlSerializerWrapper.ToXml(path, config);

            Assert.False(XmlSerializerWrapper.HasMissingSerializedMembers(path, config));
        }
        finally {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void HasMissingSerializedMembers_ReturnsTrueWhenExpectedElementIsMissing() {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        try {
            var config = new ConfigModel();
            XmlSerializerWrapper.ToXml(path, config);

            var document = XDocument.Load(path);
            document.Root?.Element("LoopLimit")?.Remove();
            document.Save(path);

            Assert.True(XmlSerializerWrapper.HasMissingSerializedMembers(path, config));
        }
        finally {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void FromXml_MapsLegacyFlatHordeNightFieldsIntoGroupedConfig() {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xml");
        const string xml = """
                           <TimeLoopConfig>
                             <ConfigVersion>1</ConfigVersion>
                             <Enabled>true</Enabled>
                             <Mode>whitelist</Mode>
                             <Players />
                             <MinPlayers>2</MinPlayers>
                             <DaysToSkip>0</DaysToSkip>
                             <LoopLimit>0</LoopLimit>
                             <ProtectHordeNights>false</ProtectHordeNights>
                             <HordeRewindGraceSeconds>123</HordeRewindGraceSeconds>
                           </TimeLoopConfig>
                           """;

        try {
            File.WriteAllText(path, xml);

            var config = XmlSerializerWrapper.FromXml<ConfigModel>(path);

            Assert.NotNull(config.HordeNightProtection);
            Assert.False(config.HordeNightProtection.Enabled);
            Assert.Equal(123, config.HordeNightProtection.RewindGraceSeconds);
        }
        finally {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
