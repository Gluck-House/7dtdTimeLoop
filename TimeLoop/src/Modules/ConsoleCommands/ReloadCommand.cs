using TimeLoop.Helpers;
using System.Collections.Generic;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class ReloadCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return "Usage:\ntl_reload\n    Reloads TimeLoop configuration from disk.";
        }

        public override string[] getCommands() {
            return new[] { "tl_reload", "timeloop_reload" };
        }

        public override string getDescription() {
            return "Reloads TimeLoop configuration from disk.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (!Helpers.CommandHelper.ValidateCount(_params, 0)) return;

            ConfigManager.Instance.ReloadFromDisk();
            SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("TimeLoop configuration reloaded."));
        }
    }
}
