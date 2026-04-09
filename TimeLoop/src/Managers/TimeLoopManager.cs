using System;
using TimeLoop.Enums;
using TimeLoop.Helpers;
using TimeLoop.Models;
using TimeLoop.Services;

namespace TimeLoop.Managers {
    public class TimeLoopManager {
        private readonly IBloodMoonService _bloodMoonService;
        private readonly IGameWorldAdapter _gameWorldAdapter;
        private readonly PlayerService _playerService;

        private int _timesLooped;
        private double _lastUpdateAt;
        private double? _hordeRewindPendingAt;

        private TimeLoopManager() : this(new GameWorldAdapter(), new PlayerService()) {
        }

        internal TimeLoopManager(IGameWorldAdapter gameWorldAdapter, PlayerService playerService) :
            this(gameWorldAdapter, new BloodMoonService(gameWorldAdapter), playerService) {
        }

        internal TimeLoopManager(IGameWorldAdapter gameWorldAdapter, IBloodMoonService bloodMoonService,
            PlayerService playerService) {
            _gameWorldAdapter = gameWorldAdapter;
            _bloodMoonService = bloodMoonService;
            _playerService = playerService;
        }

        public bool IsTimeFlowing { get; private set; } = true;

        private int HordeRewindGraceSeconds => Math.Max(0, ConfigManager.Instance.Config.HordeNightProtection.RewindGraceSeconds);

        private bool IsDaySkippable() {
            return TimeLoopPolicy.ShouldSkipCurrentLoop(IsTimeFlowing, ConfigManager.Instance.Config.DaysToSkip);
        }

        private bool IsLoopLimitReached() {
            return TimeLoopPolicy.IsLoopLimitReached(_timesLooped, ConfigManager.Instance.Config.LoopLimit);
        }

        private void RewindToPreviousDaySameTime() {
            var worldTime = _gameWorldAdapter.GetWorldTime();
            _gameWorldAdapter.SetWorldTime(worldTime > 24000UL ? worldTime - 24000UL : 0UL);
        }

        private int GetPendingHordeRewindSeconds() {
            if (_hordeRewindPendingAt == null)
                return 0;

            var remainingSeconds = HordeRewindGraceSeconds - (_gameWorldAdapter.GetUnscaledTime() - _hordeRewindPendingAt.Value);
            return Math.Max(0, (int)Math.Ceiling(remainingSeconds));
        }

        private void ClearPendingHordeNightRewind(bool notifyPlayers) {
            if (_hordeRewindPendingAt == null)
                return;

            _hordeRewindPendingAt = null;
            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_loop_horde_cancelled"));
            if (notifyPlayers)
                MessageHelper.SendGlobalChat(LocaleManager.Instance.LocalizeWithPrefix("loop_horde_cancelled"));
        }

        private bool TryStartHordeNightRewind(BloodMoonStatus bloodMoonStatus) {
            if (!TimeLoopPolicy.CanScheduleHordeNightRewind(ConfigManager.Instance.Config, bloodMoonStatus))
                return false;

            if (_hordeRewindPendingAt != null)
                return true;

            _hordeRewindPendingAt = _gameWorldAdapter.GetUnscaledTime();
            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_loop_horde_pending", HordeRewindGraceSeconds));
            MessageHelper.SendGlobalChat(LocaleManager.Instance.LocalizeWithPrefix("loop_horde_pending",
                HordeRewindGraceSeconds));
            return true;
        }

        private void CheckPendingHordeNightRewind() {
            if (_hordeRewindPendingAt == null)
                return;

            if (IsTimeFlowing) {
                ClearPendingHordeNightRewind(notifyPlayers: true);
                return;
            }

            var bloodMoonStatus = _bloodMoonService.GetStatus();
            if (!ConfigManager.Instance.Config.HordeNightProtection.Enabled) {
                ClearPendingHordeNightRewind(notifyPlayers: false);
                return;
            }

            if (!bloodMoonStatus.IsScheduledBloodMoonDay || !bloodMoonStatus.IsBeforeBloodMoonStart) {
                Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_loop_horde_not_scheduled"));
                ClearPendingHordeNightRewind(notifyPlayers: false);
                return;
            }

            if (_gameWorldAdapter.GetUnscaledTime() - _hordeRewindPendingAt.Value < HordeRewindGraceSeconds)
                return;

            _timesLooped = 0;
            _hordeRewindPendingAt = null;
            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_loop_horde_rewind"));
            MessageHelper.SendGlobalChat(LocaleManager.Instance.LocalizeWithPrefix("loop_horde_rewind"));
            RewindToPreviousDaySameTime();
        }

        public TimeLoopStatus GetStatus() {
            var playerActivity = _playerService.GetPlayerActivitySummary();
            var bloodMoonStatus = _bloodMoonService.GetStatus();

            return new TimeLoopStatus(
                IsTimeFlowing,
                ConfigManager.Instance.Config.Mode,
                playerActivity,
                ConfigManager.Instance.Config.DaysToSkip,
                ConfigManager.Instance.Config.LoopLimit,
                _timesLooped,
                _hordeRewindPendingAt != null,
                GetPendingHordeRewindSeconds(),
                bloodMoonStatus);
        }

        public void UpdateLoopState() {
            var playerActivity = _playerService.GetPlayerActivitySummary();
            var newState = TimeLoopPolicy.DetermineTimeFlowing(ConfigManager.Instance.Config, playerActivity);
            var bloodMoonStatus = _bloodMoonService.GetStatus();

            if (newState != IsTimeFlowing) {
                switch (newState) {
                    case false:
                        if (!TryStartHordeNightRewind(bloodMoonStatus))
                            MessageHelper.SendGlobalChat(LocaleManager.Instance.Localize("loopstate_update_activated"));
                        break;
                    case true:
                        if (ConfigManager.Instance.Config.DaysToSkip > 0)
                            Log.Out(LocaleManager.Instance.LocalizeWithPrefix("log_loopstate_daystoskip_reset"));
                        ConfigManager.Instance.Config.DaysToSkip = 0;
                        ClearPendingHordeNightRewind(notifyPlayers: true);
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
            _gameWorldAdapter.AdvanceWorldTime(20UL);
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
            var previousDay = GameUtils.WorldTimeToDays(_gameWorldAdapter.GetWorldTime()) - 1;
            _gameWorldAdapter.SetWorldTime(GameUtils.DaysToWorldTime(previousDay) + 20);
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
            if (Math.Abs(_lastUpdateAt - _gameWorldAdapter.GetUnscaledTime()) <= 0.1)
                return;

            CheckPendingHordeNightRewind();

            if (IsTimeFlowing)
                return;

            if (_gameWorldAdapter.GetDayTime() <= 10) {
                if (!ConfigManager.Instance.IsLoopLimitEnabled) {
                    LoopDay();
                    return;
                }

                LimitedLoop();
            }

            _lastUpdateAt = _gameWorldAdapter.GetUnscaledTime();
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
