using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;
using TimeLoop.Services;

namespace TimeLoop.Modules.ConsoleCommands {
    public class AuthorizeClientCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return "Usage:\n(whitelist mode)\ntl_auth <player_name/platform_id> <0/1> - Authorizes a player to leave the time loop.\n    <player_name/platform_id> - Player name or Platform ID of the player to authorize.\n    <0/1> - 0 to unauthorized, 1 to authorized";
        }

        public override string[] getCommands() {
            return new[] { "tl_auth", "tl_authorize", "timeloop_auth", "timeloop_authorize" };
        }

        public override string getDescription() {
            return "Authorizes a player to leave the time loop.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            if (!CommandHelper.ValidateCount(_params, 2)) return;
            if (!CommandHelper.ValidateType(_params[1], 2, out int newValue)) return;

            var playerService = new PlayerService();
            var player = playerService.GetPlayerByNameOrId(_params[0]);
            if (player == null) {
                SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("Client {0} could not be found in the database.", _params[0]));
                return;
            }

            player.IsAuthorized = newValue >= 1;
            ConfigManager.Instance.SaveToFile();
            TimeLoopManager.Instance.UpdateLoopState();
            var newState = player.IsAuthorized
                ? "authorized"
                : "unauthorized";
            SdtdConsole.Instance.Output(TimeLoopText.WithPrefix("{0} client {1} to skip the time loop", newState, player.PlayerName));
        }
    }
}
