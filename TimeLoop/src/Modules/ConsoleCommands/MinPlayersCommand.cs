using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class MinPlayersCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return "Usage:\ntl_min <amount>\n    <amount> - The minimum number of players required for time to flow normally.";
        }

        public override string[] getCommands() {
            return new[] { "tl_min", "tl_minplayers", "timeloop_minplayers" };
        }

        public override string getDescription() {
            return "(In Threshold Mode) Changes the minimum players requirement for time to flow normally";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (_params.Count == 0) {
                SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("Minimum required players: {0}",
                    ConfigManager.Instance.Config.MinPlayers));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;
            if (!CommandHelper.ValidateType(_params[0], 1, out int newValue)) return;

            ConfigManager.Instance.Config.MinPlayers = newValue;
            ConfigManager.Instance.SaveToFile();
            TimeLoopManager.Instance.UpdateLoopState();
            SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("Minimum player requirements changed to {0}", newValue));
        }
    }
}
