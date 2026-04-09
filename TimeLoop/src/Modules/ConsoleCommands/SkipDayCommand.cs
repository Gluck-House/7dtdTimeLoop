using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class SkipDayCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return LocaleManager.Instance.Localize("cmd_skipdays_help");
        }

        public override string[] getCommands() {
            return new[] { "tl_skipdays", "timeloop_skipdays" };
        }

        public override string getDescription() {
            return LocaleManager.Instance.LocalizeWithPrefix("cmd_skipdays_desc");
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (_params.Count == 0) {
                if (ConfigManager.Instance.Config.DaysToSkip == 0) {
                    SdtdConsole.Instance.Output(
                        LocaleManager.Instance.LocalizeWithPrefix("cmd_skipdays_return_disabled"));
                    return;
                }

                SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_skipdays_return_enabled",
                    ConfigManager.Instance.Config.DaysToSkip));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;
            if (!CommandHelper.ValidateType(_params[0], 1, out int days)) return;

            ConfigManager.Instance.Config.DaysToSkip = days;
            ConfigManager.Instance.SaveToFile();
            if (days == 0) {
                SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_skipdays_return_disabled"));
                return;
            }

            SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_skipdays_return_enabled", days));
        }
    }
}
