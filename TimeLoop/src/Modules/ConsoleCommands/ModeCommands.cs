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

        protected override string GetHelpText() {
            return "Usage:\ntl_mode <always|whitelist|threshold|whitelisted_threshold|0|1|2|3>\n    0 or always - Change to always-loop mode\n    1 or whitelist - Change to whitelist mode\n    2 or threshold - Change to threshold mode\n    3 or whitelisted_threshold - Change to whitelisted threshold mode";
        }

        public override string[] getCommands() {
            return new[] { "tl_mode", "timeloop_mode" };
        }

        public override string getDescription() {
            return "Changes the TimeLoop mode.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (_params.Count == 0) {
                SdtdConsole.Instance.Output(TimeLoopText.WithPrefix(
                    "Current mode: {0}",
                    TimeLoopText.ModeName(ConfigManager.Instance.Config.Mode)));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;

            if (!ModeAliases.TryGetValue(_params[0], out var newMode)) {
                SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("Invalid mode specified."));
                return;
            }

            ConfigManager.Instance.Config.Mode = newMode;
            ConfigManager.Instance.SaveToFile();
            TimeLoopManager.Instance.UpdateLoopState();
            SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("Mode changed to {0}.",
                TimeLoopText.ModeName(ConfigManager.Instance.Config.Mode)));
        }
    }
}
