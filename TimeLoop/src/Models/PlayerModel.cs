using System;
using System.Xml.Serialization;

namespace TimeLoop.Models
{
    [Serializable]
    public class PlayerModel
    {
        [XmlAttribute("ID")]
        public string Id { get; set; }

        [XmlAttribute("Name")]
        public string PlayerName { get; set; }

        [XmlAttribute("Whitelisted")]
        public bool IsAuthorized { get; set; }

        public PlayerModel()
        {
            Id = Guid.NewGuid().ToString();
            PlayerName = string.Empty;
            IsAuthorized = false;
        }

        public PlayerModel(ClientInfo clientInfo)
        {
            Id = clientInfo.PlatformId.CombinedString;
            PlayerName = clientInfo.playerName;
            IsAuthorized = false;
        }
    }
}
