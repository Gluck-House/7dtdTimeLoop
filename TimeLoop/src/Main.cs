using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop {
    public class Main : IModApi {
        public const string ConfigFilePath = "TimeLooper.xml";

        public void InitMod(Mod _modInstance) {
            Log.Out("[TimeLoop] Initializing ...");
            ModEvents.GameAwake.RegisterHandler(Awake);
            ModEvents.GameUpdate.RegisterHandler(Update);
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(OnPlayerRespawn);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        public static string GetAbsolutePath(string relativeFilePath, bool requireExists = true) {
            var gameDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName;
            if (gameDirectory == null) {
                Log.Exception(new Exception("Game directory could not be found."));
                throw new Exception("Game directory could not be found.");
            }

            var filePath = Path.Combine(gameDirectory, relativeFilePath);
            if (!requireExists)
                return filePath;

            if (File.Exists(filePath))
                return filePath;

            if (Directory.Exists(filePath))
                return filePath;

            throw new Exception($"File {filePath} could not be found. Check the mods folder");
        }

        public static bool IsDedicatedServer() {
            return GameManager.Instance && GameManager.IsDedicatedServer;
        }

        private void Awake(ref ModEvents.SGameAwakeData data) {
            if (!IsDedicatedServer())
                return;

            ConfigManager.Instantiate();
            TimeLoopManager.Instantiate();
        }

        private void Update(ref ModEvents.SGameUpdateData data) {
            if (!IsDedicatedServer())
                return;

            ConfigManager.Instance.UpdateFromFile();
            if (ConfigManager.Instance.Config.Enabled)
                TimeLoopManager.Instance.CheckForTimeLoop();
        }

        private void OnPlayerRespawn(ref ModEvents.SPlayerSpawnedInWorldData data) {
            if (!IsDedicatedServer())
                return;

            if (!ConfigManager.Instance.Config.Enabled)
                return;

            if (data.RespawnType != RespawnType.JoinMultiplayer)
                return;

            if (TimeLoopManager.Instance.IsTimeFlowing)
                return;

            MessageHelper.SendPrivateChat("TimeLoop is active. Day will reset at midnight.", data.ClientInfo);
        }
    }
}
