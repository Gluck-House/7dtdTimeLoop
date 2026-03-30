using System;
using TimeLoop.Enums;
using TimeLoop.Helpers;
using TimeLoop.Repositories;
using UnityEngine;

namespace TimeLoop.Managers {
    public class TimeLoopManager {
        private int _timesLooped;

        private double _unscaledTimeStamp;
        public bool IsTimeFlowing { get; private set; } = true;

        private bool IsDaySkippable() {
            return !IsTimeFlowing && ConfigManager.Instance.Config.DaysToSkip > 0;
        }

        private bool IsLoopLimitReached() {
            return _timesLooped >= ConfigManager.Instance.Config.LoopLimit && ConfigManager.Instance.IsLoopLimitEnabled;
        }

        public void UpdateLoopState() {
            var plyDataRepo = new PlayerRepository();
            var newState = ConfigManager.Instance.Config.Enabled && ConfigManager.Instance.Config.Mode switch {
                EMode.Whitelist => plyDataRepo.IsAuthPlayerOnline(),
                EMode.Threshold => plyDataRepo.IsMinPlayerThreshold(),
                EMode.WhitelistedThreshold => plyDataRepo.IsMinAuthPlayerThreshold(),
                EMode.Always => false,
                _ => false
            };

            if (newState != IsTimeFlowing) {
                switch (newState) {
                    case false:
                        MessageHelper.SendGlobalChat(LocaleManager.Instance.Localize("loopstate_update_activated"));
                        break;
                    case true:
                        if (ConfigManager.Instance.Config.DaysToSkip > 0)
                            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_loopstate_daystoskip_reset"));
                        ConfigManager.Instance.Config.DaysToSkip = 0;
                        ConfigManager.Instance.SaveToFile();
                        MessageHelper.SendGlobalChat(LocaleManager.Instance.Localize("loopstate_update_deactivated"));
                        break;
                }

                IsTimeFlowing = newState;
            }

            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_loopstate_status", newState,
                ConfigManager.Instance.Config.DaysToSkip));
        }

        private void SkipLoop() {
            _timesLooped = 0;
            GameManager.Instance.World.worldTime += 20;
            MessageHelper.SendGlobalChat(LocaleManager.Instance.LocalizeWithPrefix("loop_dayloop"));
            if (ConfigManager.Instance.DecreaseDaysToSkip() > 0)
                MessageHelper.SendGlobalChat(LocaleManager.Instance.LocalizeWithPrefix("loop_daystoskip_active",
                    ConfigManager.Instance.Config.DaysToSkip));
            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_loop_daystoskip_active",
                ConfigManager.Instance.Config.DaysToSkip));
        }

        private void LoopDay() {
            if (IsDaySkippable()) {
                SkipLoop();
                return;
            }

            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_loop_dayloop"));
            MessageHelper.SendGlobalChat(LocaleManager.Instance.LocalizeWithPrefix("loop_dayloop"));
            var previousDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.GetWorldTime()) - 1;
            GameManager.Instance.World.SetTime(GameUtils.DaysToWorldTime(previousDay) + 20);
        }

        private void LimitedLoop() {
            if (!IsLoopLimitReached()) {
                LoopDay();
                _timesLooped++;
                Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_loop_limit", _timesLooped,
                    ConfigManager.Instance.Config.LoopLimit));
                return;
            }

            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("loop_limitreached"));
            MessageHelper.SendGlobalChat(LocaleManager.Instance.LocalizeWithPrefix("loop_limitreached"));
            SkipLoop();
        }


        public void CheckForTimeLoop() {
            if (Math.Abs(_unscaledTimeStamp - Time.unscaledTimeAsDouble) <= 0.1)
                return;

            if (IsTimeFlowing)
                return;

            var worldTime = GameManager.Instance.World.GetWorldTime();
            var dayTime = worldTime % 24000;

            if (dayTime <= 10) {
                if (!ConfigManager.Instance.IsLoopLimitEnabled) {
                    LoopDay();
                    return;
                }

                LimitedLoop();
            }

            _unscaledTimeStamp = Time.unscaledTimeAsDouble;
        }

        public static implicit operator bool(TimeLoopManager? instance) {
            return instance != null;
        }

        #region Singleton

        private static TimeLoopManager? _instance;

        public static TimeLoopManager Instance {
            get { return _instance ??= new TimeLoopManager(); }
        }

        public static void Instantiate() {
            _instance = new TimeLoopManager();
        }

        #endregion
    }
}