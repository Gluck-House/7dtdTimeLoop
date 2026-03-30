using System.Xml.Serialization;

namespace TimeLoop.Enums
{
    public enum EMode
    {
        [XmlEnum(Name = "always")]
        Always,
        [XmlEnum(Name = "whitelist")]
        Whitelist,
        [XmlEnum(Name = "threshold")]
        Threshold,
        [XmlEnum(Name = "whitelisted_threshold")]
        WhitelistedThreshold
    }
}