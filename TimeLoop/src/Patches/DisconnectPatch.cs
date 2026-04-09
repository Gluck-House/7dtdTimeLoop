using HarmonyLib;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Patches {
    [HarmonyPatch(typeof(ConnectionManager), nameof(ConnectionManager.DisconnectClient))]
    public class DisconnectPatch {
        private static void Postfix(ConnectionManager __instance) {
            if (!Main.IsDedicatedServer())
                return;
            Log.Out(TimeLoopText.WithPrefix("Player disconnected. Updating loop parameters."));
            TimeLoopManager.Instance.UpdateLoopState();
        }
    }
}
