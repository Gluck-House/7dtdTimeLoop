using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;
using TimeLoop.Services;

namespace TimeLoop.Modules.ConsoleCommands {
    public class AuthorizeClientCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return LocaleManager.Instance.Localize("cmd_authorize_help");
        }

        public override string[] getCommands() {
            return new[] { "tl_auth", "tl_authorize", "timeloop_auth", "timeloop_authorize" };
        }

        public override string getDescription() {
            return LocaleManager.Instance.LocalizeWithPrefix("cmd_authorize_desc");
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (!CommandHelper.ValidateCount(_params, 2)) return;
            if (!CommandHelper.ValidateType(_params[1], 2, out int newValue)) return;

            var playerService = new PlayerService();
            var player = playerService.GetPlayerByNameOrId(_params[0]);
            if (player == null) {
                SdtdConsole.Instance.Output(
                    LocaleManager.Instance.LocalizeWithPrefix("cmd_player_not_found", _params[0]));
                return;
            }

            player.IsAuthorized = newValue >= 1;
            ConfigManager.Instance.SaveToFile();
            TimeLoopManager.Instance.UpdateLoopState();
            var newState = player.IsAuthorized
                ? LocaleManager.Instance.Localize("authorized")
                : LocaleManager.Instance.Localize("unauthorized");
            SdtdConsole.Instance.Output(
                LocaleManager.Instance.LocalizeWithPrefix("cmd_authorized_return", newState, player.PlayerName));
        }
    }
}
