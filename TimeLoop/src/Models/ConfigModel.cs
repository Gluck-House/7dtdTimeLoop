using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TimeLoop.Enums;

namespace TimeLoop.Models {
    [Serializable]
    [XmlRoot("TimeLoopConfig")]
    public class ConfigModel {
        public const int CurrentVersion = 2;

        public ConfigModel() {
            ConfigVersion = CurrentVersion;
            Enabled = true;
            Mode = EMode.Whitelist;
            Players = new List<PlayerModel>();
            MinPlayers = 5;
            DaysToSkip = 0;
            LoopLimit = 0;
            HordeNightProtection = new HordeNightProtectionConfig();
            Language = "en_us";
        }

        public ConfigModel(int configVersion, bool enabled, EMode mode, List<PlayerModel> players, int minPlayers,
            int daysToSkip, int loopLimit, HordeNightProtectionConfig hordeNightProtection, string language) {
            ConfigVersion = configVersion;
            Enabled = enabled;
            Mode = mode;
            Players = players;
            MinPlayers = minPlayers;
            DaysToSkip = daysToSkip;
            LoopLimit = loopLimit;
            HordeNightProtection = hordeNightProtection ?? new HordeNightProtectionConfig();
            Language = language;
        }

        [XmlElement("ConfigVersion")] public int ConfigVersion { get; set; }

        [XmlElement("Enabled")] public bool Enabled { get; set; }

        [XmlElement("Mode")] public EMode Mode { get; set; }

        [XmlArray("Players")] public List<PlayerModel> Players { get; set; }

        [XmlElement("MinPlayers")] public int MinPlayers { get; set; }

        [XmlElement("DaysToSkip")] public int DaysToSkip { get; set; }

        [XmlElement("LoopLimit")] public int LoopLimit { get; set; }

        [XmlElement("HordeNightProtection")] public HordeNightProtectionConfig HordeNightProtection { get; set; }

        [XmlElement("ProtectHordeNights")] public bool LegacyProtectHordeNights {
            get => HordeNightProtection.Enabled;
            set => HordeNightProtection.Enabled = value;
        }

        public bool ShouldSerializeLegacyProtectHordeNights() {
            return false;
        }

        [XmlElement("HordeRewindGraceSeconds")] public int LegacyHordeRewindGraceSeconds {
            get => HordeNightProtection.RewindGraceSeconds;
            set => HordeNightProtection.RewindGraceSeconds = value;
        }

        public bool ShouldSerializeLegacyHordeRewindGraceSeconds() {
            return false;
        }

        [XmlElement("Language")] public string Language { get; set; }
    }
}
