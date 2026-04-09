using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class SkipDayCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return "Usage:\ntl_skipdays <days>\n    <days> The amount of days to skip looping.";
        }

        public override string[] getCommands() {
            return new[] { "tl_skipdays", "timeloop_skipdays" };
        }

        public override string getDescription() {
            return "Skip the looping for N amount of days.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (_params.Count == 0) {
                if (ConfigManager.Instance.Config.DaysToSkip == 0) {
                    SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("No days will skip the loop."));
                    return;
                }

                SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("The following {0} day(s) will skip the loop",
                    ConfigManager.Instance.Config.DaysToSkip));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;
            if (!CommandHelper.ValidateType(_params[0], 1, out int days)) return;

            ConfigManager.Instance.Config.DaysToSkip = days;
            ConfigManager.Instance.SaveToFile();
            if (days == 0) {
                SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("No days will skip the loop."));
                return;
            }

            SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("The following {0} day(s) will skip the loop", days));
        }
    }
}
