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
            return LocaleManager.Instance.Localize("cmd_list_help");
        }

        public override string[] getCommands() {
            return new[] { "tl_list", "timeloop_list" };
        }

        public override string getDescription() {
            return LocaleManager.Instance.LocalizeWithPrefix("cmd_list_desc");
        }

        private string FormatPlayerList(List<PlayerModel> players) {
            if (players.Count == 0)
                return LocaleManager.Instance.Localize("cmd_list_no_users");

            var formattedPlayers = string.Join(
                Environment.NewLine,
                players.Select(
                    player => LocaleManager.Instance.Localize("cmd_list_format", player.PlayerName, player.Id,
                        player.IsAuthorized)));
            return LocaleManager.Instance.LocalizeWithPrefix("cmd_list_return", formattedPlayers + Environment.NewLine,
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
