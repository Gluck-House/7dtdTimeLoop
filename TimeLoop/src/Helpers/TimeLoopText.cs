using System;
using TimeLoop.Enums;

namespace TimeLoop.Helpers {
    public static class TimeLoopText {
        public const string Prefix = "[TimeLoop] ";

        public static string WithPrefix(string message) {
            return Prefix + message;
        }

        public static string WithPrefix(string format, params object[] args) {
            return Prefix + string.Format(format, args);
        }

        public static string ModeName(EMode mode) {
            return mode switch {
                EMode.Always => "Always",
                EMode.Whitelist => "Whitelist",
                EMode.Threshold => "Threshold",
                EMode.WhitelistedThreshold => "Whitelisted Threshold",
                _ => "Unknown"
            };
        }
    }
}
