using System;
using System.Collections.Generic;
using System.Linq;
using TimeLoop.Managers;
using TimeLoop.Models;

namespace TimeLoop.Services {
    public class PlayerService {
        public PlayerModel? GetPlayerByNameOrId(string nameOrId) {
            if (string.IsNullOrWhiteSpace(nameOrId))
                return null;

            return ConfigManager.Instance.Config.Players.Find(player =>
                player != null && (
                    string.Equals(player.PlayerName, nameOrId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(player.Id, nameOrId, StringComparison.OrdinalIgnoreCase)));
        }

        public PlayerModel? GetPlayer(ClientInfo clientInfo) {
            return ConfigManager.Instance.Config.Players.Find(player => {
                if (player == null)
                    return false;

                if (player.Id == clientInfo.CrossplatformId?.CombinedString)
                    return true;

                return clientInfo.PlatformId.CombinedString == player.Id;
            });
        }

        public PlayerActivitySummary GetPlayerActivitySummary() {
            var clients = GetConnectedClients();
            return new PlayerActivitySummary(clients.Count, clients.Count(IsClientAuthorized));
        }

        public bool IsAuthorizedPlayerOnline() {
            return GetConnectedClients().Any(IsClientAuthorized);
        }

        public bool IsMinPlayerThresholdMet() {
            return GetConnectedClients().Count >= ConfigManager.Instance.Config.MinPlayers;
        }

        public bool IsMinAuthorizedPlayerThresholdMet() {
            var clients = GetConnectedClients();
            return clients.Count(IsClientAuthorized) >= ConfigManager.Instance.Config.MinPlayers;
        }

        public List<PlayerModel> GetAllUsers() {
            return ConfigManager.Instance.Config.Players.FindAll(player => !string.IsNullOrWhiteSpace(player.PlayerName));
        }

        public List<PlayerModel> GetAllAuthorizedUsers(bool unauthorizedInstead = false) {
            return ConfigManager
                .Instance
                .Config
                .Players
                .FindAll(player => !string.IsNullOrWhiteSpace(player.PlayerName) && player.IsAuthorized == !unauthorizedInstead);
        }

        private List<ClientInfo> GetConnectedClients() {
            ConnectionManager.Instance.LateUpdate();
            if (ConnectionManager.Instance.Clients != null && ConnectionManager.Instance.Clients.Count > 0)
                return ConnectionManager.Instance.Clients.List.Where(client =>
                    client is { loginDone: true, disconnecting: false } &&
                    (client.CrossplatformId != null || client.PlatformId != null)).ToList();
            return new List<ClientInfo>();
        }

        private bool IsClientAuthorized(ClientInfo clientInfo) {
            var player = GetPlayer(clientInfo);
            if (player != null)
                return player.IsAuthorized;
            Log.Error(LocaleManager.Instance.LocalizeWithPrefix("log_player_data_not_found", clientInfo.playerName));
            return false;
        }
    }
}
