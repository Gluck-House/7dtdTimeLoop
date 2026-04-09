using System;
using System.Xml.Serialization;

namespace TimeLoop.Models {
    [Serializable]
    public class HordeNightProtectionConfig {
        public HordeNightProtectionConfig() {
            Enabled = true;
            RewindGraceSeconds = 300;
        }

        public HordeNightProtectionConfig(bool enabled, int rewindGraceSeconds) {
            Enabled = enabled;
            RewindGraceSeconds = rewindGraceSeconds;
        }

        [XmlElement("Enabled")] public bool Enabled { get; set; }

        [XmlElement("RewindGraceSeconds")] public int RewindGraceSeconds { get; set; }
    }
}
