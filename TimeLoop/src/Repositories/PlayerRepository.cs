using System.Collections.Generic;
using System.Linq;
using TimeLoop.Managers;
using TimeLoop.Models;

namespace TimeLoop.Repositories {
    public class PlayerRepository {
        public PlayerModel? GetPlayerDataNameOrId(string nameOrId) {
            if (string.IsNullOrEmpty(nameOrId))
                return null;

            return ConfigManager.Instance.Config.Players.Find(data => {
                if (data == null)
                    return false;

                return data.playerName.Equals(nameOrId) || data.id.Equals(nameOrId);
            });
        }

        public PlayerModel? GetPlayerData(ClientInfo clientInfo) {
            return ConfigManager.Instance.Config.Players.Find(data => {
                if (data == null)
                    return false;

                if (data.id == clientInfo.CrossplatformId?.CombinedString)
                    return true;

                return clientInfo.PlatformId.CombinedString == data.id;
            });
        }

        public bool IsAuthPlayerOnline() {
            var clients = GetConnectedClients();
            return clients.Any(IsClientAuthorized);
        }

        public bool IsMinPlayerThreshold() {
            var clients = GetConnectedClients();
            return clients.Count >= ConfigManager.Instance.Config.MinPlayers;
        }

        public bool IsMinAuthPlayerThreshold() {
            var clients = GetConnectedClients();
            var authorizedClientCount = clients.Count(IsClientAuthorized);
            return authorizedClientCount >= ConfigManager.Instance.Config.MinPlayers;
        }

        public List<PlayerModel> GetAllUsers() {
            return ConfigManager.Instance.Config.Players.FindAll(data => data.playerName.Count() > 1);
        }

        public List<PlayerModel> GetAllAuthorizedUsers(bool unauthorizedInstead = false) {
            return ConfigManager
                .Instance
                .Config
                .Players
                .FindAll(data => data.playerName.Count() > 1 && data.skipTimeLoop == !unauthorizedInstead);
        }

        private List<ClientInfo> GetConnectedClients() {
            ConnectionManager.Instance.LateUpdate();
            if (ConnectionManager.Instance.Clients != null && ConnectionManager.Instance.Clients.Count > 0)
                return ConnectionManager.Instance.Clients.List.Where(x =>
                    x is { loginDone: true, disconnecting: false } &&
                    (x.CrossplatformId != null || x.PlatformId != null)).ToList();
            return new List<ClientInfo>();
        }

        private bool IsClientAuthorized(ClientInfo cInfo) {
            var plyData = GetPlayerData(cInfo);
            if (plyData != null)
                return plyData.skipTimeLoop;
            Log.Error(LocaleManager.Instance.LocalizeWithPrefix("log_player_data_not_found", cInfo.playerName));
            return false;
        }
    }
}