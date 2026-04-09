using System.Collections.Generic;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class ReloadCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return LocaleManager.Instance.Localize("cmd_reload_help");
        }

        public override string[] getCommands() {
            return new[] { "tl_reload", "timeloop_reload" };
        }

        public override string getDescription() {
            return LocaleManager.Instance.LocalizeWithPrefix("cmd_reload_desc");
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (!Helpers.CommandHelper.ValidateCount(_params, 0)) return;

            ConfigManager.Instance.ReloadFromDisk();
            SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_reload_return"));
        }
    }
}
