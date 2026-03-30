using HarmonyLib;
using TimeLoop.Managers;
using TimeLoop.Models;
using TimeLoop.Repositories;

namespace TimeLoop.Patches {
    [HarmonyPatch(typeof(AuthorizationManager), nameof(AuthorizationManager.playerAllowed))]
    public class PlayerAllowedPatch {
        private static void Postfix(ClientInfo _clientInfo, AuthorizationManager __instance) {
            if (!Main.IsDedicatedServer())
                return;

            if (_clientInfo.PlatformId == null)
                return;
            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_player_connected"));
            TimeLoopManager.Instance.UpdateLoopState();

            var playerData = new PlayerRepository().GetPlayerData(_clientInfo);

            if (playerData != null)
                return;

            var plyData = new PlayerModel(_clientInfo);
            ConfigManager.Instance.Config.Players.Add(plyData);
            ConfigManager.Instance.SaveToFile();

            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_player_new", plyData.playerName, plyData.id));
        }
    }
}