using System;
using System.Collections.Generic;
using TimeLoop.Enums;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class ModeCommands : ConsoleCmdAbstract {
        public override string getHelp() {
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
                    LocaleManager.Instance.Localize(ConfigManager.Instance.Config.Mode.ToString().ToLower())));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;
            if (!CommandHelper.ValidateType(_params[0], 1, out int mode)) return;

            if (!Enum.TryParse<EMode>(mode.ToString(), out var newMode)) {
                SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_mode_invalid_mode"));
                return;
            }

            ConfigManager.Instance.Config.Mode = newMode;
            ConfigManager.Instance.SaveToFile();
            TimeLoopManager.Instance.UpdateLoopState();
            SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_mode_return",
                LocaleManager.Instance.Localize(ConfigManager.Instance.Config.Mode.ToString().ToLower())));
        }
    }
}