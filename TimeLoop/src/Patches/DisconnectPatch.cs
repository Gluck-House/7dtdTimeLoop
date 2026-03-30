using HarmonyLib;
using TimeLoop.Managers;

namespace TimeLoop.Patches {
    [HarmonyPatch(typeof(ConnectionManager), nameof(ConnectionManager.DisconnectClient))]
    public class DisconnectPatch {
        private static void Postfix(ConnectionManager __instance) {
            if (!Main.IsDedicatedServer())
                return;
            Log.Out(LocaleManager.Instance.Localize("log_player_disconnected"));
            TimeLoopManager.Instance.UpdateLoopState();
        }
    }
}