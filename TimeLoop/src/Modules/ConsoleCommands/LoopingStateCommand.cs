using System.Collections.Generic;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class LoopingStateCommand : ConsoleCmdAbstract {
        public override string getHelp() {
            return LocaleManager.Instance.Localize("cmd_loopstate_help");
        }

        public override string[] getCommands() {
            return new[] { "tl_state", "timeloop_state" };
        }

        public override string getDescription() {
            return LocaleManager.Instance.LocalizeWithPrefix("cmd_loopstate_desc");
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            var isOrNot = TimeLoopManager.Instance.IsTimeFlowing ? "is_not" : "is";
            SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_loopstate_return",
                LocaleManager.Instance.Localize(isOrNot)));
        }
    }
}