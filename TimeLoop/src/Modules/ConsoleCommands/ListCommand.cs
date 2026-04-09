using System;
using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;
using TimeLoop.Models;
using TimeLoop.Services;
using UniLinq;

namespace TimeLoop.Modules.ConsoleCommands {
    public class ListCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return "Usage:\ntl_list <all/auth/unauth>:\n    all - Lists all users in database\n    auth - Lists all authorized users\n    unauth - Lists all unauthorized users";
        }

        public override string[] getCommands() {
            return new[] { "tl_list", "timeloop_list" };
        }

        public override string getDescription() {
            return "Lists all users in the database.";
        }

        private string FormatPlayerList(List<PlayerModel> players) {
            if (players.Count == 0)
                return "No users in database";

            var formattedPlayers = string.Join(
                Environment.NewLine,
                players.Select(
                    player => string.Format("Player: {0}, Platform ID: {1}, Authorized? {2}",
                        player.PlayerName, player.Id, player.IsAuthorized)));
            return TimeLoopText.WithPrefix("{0}\nTotal: {1}", formattedPlayers,
                players.Count);
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            string[] validParams = { "auth", "unauth", "all" };
            var playerService = new PlayerService();

            if (_params.Count == 0 || _params[0].ToLower() == "all") {
                SdtdConsole.Instance.Output(FormatPlayerList(playerService.GetAllUsers()));
                return;
            }

            if (!CommandHelper.ValidateCount(_params, 1)) return;
            if (!CommandHelper.HasValue(_params[0].ToLower(), validParams)) return;

            var unauthorizedInstead = _params[0].ToLower().Equals("unauth");
            SdtdConsole.Instance.Output(FormatPlayerList(playerService.GetAllAuthorizedUsers(unauthorizedInstead)));
        }
    }
}
