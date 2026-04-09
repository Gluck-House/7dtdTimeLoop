using System;
using System.IO;
using System.Xml;
using TimeLoop.Helpers;
using TimeLoop.Models;
using TimeLoop.Services;
using TimeLoop.Wrappers;

namespace TimeLoop.Managers {
    public class ConfigManager {
        private readonly string _absoluteFilePath;
        private DateTime _lastModified = new DateTime(1970, 1, 1);

        private ConfigManager(string fileLocation) {
            _absoluteFilePath = Main.GetAbsolutePath(fileLocation, requireExists: false);
            Config = LoadConfig();
        }

        public ConfigModel Config { get; }

        public bool IsLoopLimitEnabled => Config.LoopLimit > 0;

        private bool ConfigNeedsMigration(ConfigModel config) {
            return File.Exists(_absoluteFilePath) && XmlSerializerWrapper.HasMissingSerializedMembers(_absoluteFilePath, config);
        }

        private void RefreshLastModified() {
            if (File.Exists(_absoluteFilePath))
                _lastModified = new FileInfo(_absoluteFilePath).LastWriteTime;
        }

        private void PersistConfigIfNeeded(bool shouldPersist) {
            if (!shouldPersist)
                return;

            SaveToFile();
        }

        private bool IsFileModified() {
            return _lastModified != new FileInfo(_absoluteFilePath).LastWriteTime;
        }

        private ConfigModel LoadConfig() {
            var configModel = new ConfigModel();
            var shouldPersist = false;
            try {
                Log.Out("[TimeLoop] Loading configuration file...");
                configModel = XmlSerializerWrapper.FromXml<ConfigModel>(_absoluteFilePath);
                shouldPersist = ConfigMigrationService.Migrate(configModel) || ConfigNeedsMigration(configModel);
            }
            catch (Exception e) when (e is FileNotFoundException || e is XmlException) {
                Log.Error("[TimeLoop] Configuration file is either corrupt or does not exist.");
                Log.Out("[TimeLoop] Creating a configuration file");
                XmlSerializerWrapper.ToXml(_absoluteFilePath, configModel);
            }
            finally {
                RefreshLastModified();
                Log.Out("[TimeLoop] Configuration loaded.");
            }

            if (shouldPersist)
                XmlSerializerWrapper.ToXml(_absoluteFilePath, configModel);

            RefreshLastModified();

            return configModel;
        }

        public void UpdateFromFile() {
            if (!File.Exists(_absoluteFilePath))
                return;

            if (!IsFileModified())
                return;

            XmlSerializerWrapper.FromXmlOverwrite(_absoluteFilePath, Config);
            PersistConfigIfNeeded(ConfigMigrationService.Migrate(Config) || ConfigNeedsMigration(Config));
            RefreshLastModified();
            Log.Out(TimeLoopText.WithPrefix("Configuration file updated."));
            TimeLoopManager.Instance.UpdateLoopState();
        }

        public void ReloadFromDisk() {
            if (!File.Exists(_absoluteFilePath))
                return;

            _lastModified = new DateTime(1970, 1, 1);
            UpdateFromFile();
        }

        public void SaveToFile() {
            if (!File.Exists(_absoluteFilePath))
                return;

            XmlSerializerWrapper.ToXml(_absoluteFilePath, Config);
            RefreshLastModified();
        }

        public int DecreaseDaysToSkip() {
            if (Config.DaysToSkip == 0)
                return 0;
            Config.DaysToSkip--;
            SaveToFile();
            return Config.DaysToSkip;
        }

        public static implicit operator bool(ConfigManager? instance) {
            return instance != null;
        }

        #region Singleton

        private static ConfigManager? _instance;

        public static ConfigManager Instance {
            get { return _instance ??= new ConfigManager(Main.ConfigFilePath); }
        }

        public static void Instantiate() {
            _instance = new ConfigManager(Main.ConfigFilePath);
        }

        #endregion
    }
}
