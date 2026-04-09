using System;
using System.Collections.Generic;

namespace TimeLoop.Helpers
{
    public static class MessageHelper
    {
        private static readonly EChatType PrivateChatType = ResolvePrivateChatType();

        private static EChatType ResolvePrivateChatType()
        {
            foreach (var candidate in new[] { "Whisper", "PM", "Private" })
            {
                if (Enum.TryParse(candidate, out EChatType chatType))
                    return chatType;
            }

            return EChatType.Global;
        }

        public static void SendGlobalChat(string message)
        {
            GameManager.Instance.ChatMessageServer(null, EChatType.Global, -1, message, null, EMessageSender.None);
        }

        public static void SendPrivateChat(string message, ClientInfo recipient)
        {
            GameManager.Instance.ChatMessageServer(null, PrivateChatType, -1, message, new List<int>{ recipient.entityId }, EMessageSender.None);
        }
    }
}
