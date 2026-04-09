using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class LocaleCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return LocaleManager.Instance.Localize("cmd_locale_help");
        }

        public override string[] getCommands() {
            return new[] { "tl_locale", "timeloop_locale" };
        }

        public override string getDescription() {
            return LocaleManager.Instance.LocalizeWithPrefix("cmd_locale_desc");
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (_params.Count == 0) {
                SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_locale_state",
                    LocaleManager.Instance.LoadedLocale, LocaleManager.Instance.LocaleList.Join()));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;
            if (!CommandHelper.HasValue(_params[0], LocaleManager.Instance.LocaleList.ToArray())) return;

            LocaleManager.Instance.SetLocale(_params[0]);
            ConfigManager.Instance.Config.Language = LocaleManager.Instance.LoadedLocale;
            ConfigManager.Instance.SaveToFile();
            SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_locale_return", _params[0]));
        }
    }
}
