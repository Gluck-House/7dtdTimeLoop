using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class LoopLimitCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return "Usage:\ntl_looplimit <amount>\n    <amount> - The amount of loops a day can have. 0 to loop indefinitely.";
        }

        public override string[] getCommands() {
            return new[] { "tl_ll", "tl_looplimit", "timeloop_looplimit" };
        }

        public override string getDescription() {
            return "Limit the amount of loops a day can have.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (_params.Count == 0) {
                var loopLimit = ConfigManager.Instance.Config.LoopLimit > 0
                    ? ConfigManager.Instance.Config.LoopLimit.ToString()
                    : "infinite";
                SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("Current loop limit is {0}", loopLimit));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;
            if (!CommandHelper.ValidateType(_params[0], 1, out int newValue)) return;

            ConfigManager.Instance.Config.LoopLimit = newValue;
            ConfigManager.Instance.SaveToFile();
            TimeLoopManager.Instance.UpdateLoopState();
            SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("Loop limit changed to {0}", newValue));
        }
    }
}
