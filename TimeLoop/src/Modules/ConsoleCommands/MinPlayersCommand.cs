using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class MinPlayersCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return LocaleManager.Instance.Localize("cmd_minplayers_help");
        }

        public override string[] getCommands() {
            return new[] { "tl_min", "tl_minplayers", "timeloop_minplayers" };
        }

        public override string getDescription() {
            return LocaleManager.Instance.LocalizeWithPrefix("cmd_minplayers_desc");
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (_params.Count == 0) {
                SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_minplayers_state",
                    ConfigManager.Instance.Config.MinPlayers));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;
            if (!CommandHelper.ValidateType(_params[0], 1, out int newValue)) return;

            ConfigManager.Instance.Config.MinPlayers = newValue;
            ConfigManager.Instance.SaveToFile();
            TimeLoopManager.Instance.UpdateLoopState();
            SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_minplayers_return", newValue));
        }
    }
}
