using HarmonyLib;
using TimeLoop.Helpers;
using TimeLoop.Managers;
using TimeLoop.Models;
using TimeLoop.Services;

namespace TimeLoop.Patches {
    [HarmonyPatch(typeof(AuthorizationManager), nameof(AuthorizationManager.playerAllowed))]
    public class PlayerAllowedPatch {
        private static void Postfix(ClientInfo _clientInfo, AuthorizationManager __instance) {
            if (!Main.IsDedicatedServer())
                return;

            if (_clientInfo.PlatformId == null)
                return;
            Log.Out(TimeLoopText.WithPrefix("Player logged in. Updating loop parameters."));
            TimeLoopManager.Instance.UpdateLoopState();

            var playerData = new PlayerService().GetPlayer(_clientInfo);

            if (playerData != null)
                return;

            var player = new PlayerModel(_clientInfo);
            ConfigManager.Instance.Config.Players.Add(player);
            ConfigManager.Instance.SaveToFile();

            Log.Out(TimeLoopText.WithPrefix("Player {0} ({1}) added to config", player.PlayerName, player.Id));
        }
    }
}
