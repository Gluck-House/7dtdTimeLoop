using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class EnableCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return "Usage:\ntl_enable <0/1>\n    0 - Disables the mod.\n    1 - Enables the mod.";
        }

        public override string[] getCommands() {
            return new[] { "tl_enable", "timeloop_enable", "timeloop" };
        }

        public override string getDescription() {
            return "Enables or disables the mod.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (_params.Count == 0) {
                SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("Is mod enabled? {0}", ConfigManager.Instance.Config.Enabled));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;
            if (!CommandHelper.ValidateType(_params[0], 1, out int newValue)) return;

            ConfigManager.Instance.Config.Enabled = newValue >= 1;
            ConfigManager.Instance.SaveToFile();
            TimeLoopManager.Instance.UpdateLoopState();
            var newState = ConfigManager.Instance.Config.Enabled
                ? "enabled"
                : "disabled";
            SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("TimeLoop has been {0}", newState));
        }
    }
}
