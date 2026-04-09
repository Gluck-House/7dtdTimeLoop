using System;
using System.Collections.Generic;
using TimeLoop.Enums;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class ModeCommands : TimeLoopConsoleCommandBase {
        private static readonly Dictionary<string, EMode> ModeAliases = new Dictionary<string, EMode>(StringComparer.OrdinalIgnoreCase) {
            ["0"] = EMode.Always,
            ["1"] = EMode.Whitelist,
            ["2"] = EMode.Threshold,
            ["3"] = EMode.WhitelistedThreshold,
            ["always"] = EMode.Always,
            ["whitelist"] = EMode.Whitelist,
            ["threshold"] = EMode.Threshold,
            ["whitelisted_threshold"] = EMode.WhitelistedThreshold,
            ["whitelisted-threshold"] = EMode.WhitelistedThreshold
        };

        private static string GetModeLocaleKey(EMode mode) {
            return mode switch {
                EMode.Always => "always",
                EMode.Whitelist => "whitelist",
                EMode.Threshold => "threshold",
                EMode.WhitelistedThreshold => "whitelisted_threshold",
                _ => "cmd_mode_invalid_mode"
            };
        }

        protected override string GetHelpText() {
            return LocaleManager.Instance.Localize("cmd_mode_help");
        }

        public override string[] getCommands() {
            return new[] { "tl_mode", "timeloop_mode" };
        }

        public override string getDescription() {
            return LocaleManager.Instance.Localize("cmd_mode_desc");
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (_params.Count == 0) {
                SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix(
                    "cmd_mode_state",
                    LocaleManager.Instance.Localize(GetModeLocaleKey(ConfigManager.Instance.Config.Mode))));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;

            if (!ModeAliases.TryGetValue(_params[0], out var newMode)) {
                SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_mode_invalid_mode"));
                return;
            }

            ConfigManager.Instance.Config.Mode = newMode;
            ConfigManager.Instance.SaveToFile();
            TimeLoopManager.Instance.UpdateLoopState();
            SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_mode_return",
                LocaleManager.Instance.Localize(GetModeLocaleKey(ConfigManager.Instance.Config.Mode))));
        }
    }
}
