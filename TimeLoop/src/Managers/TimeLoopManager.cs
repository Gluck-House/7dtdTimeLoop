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
            Log.Out(TimeLoopText.WithPrefix("Horde-night rewind cancelled."));
            if (notifyPlayers)
                MessageHelper.SendGlobalChat(TimeLoopText.WithPrefix("Horde-night rewind cancelled. Time flows normally."));
        }

        private bool TryStartHordeNightRewind(BloodMoonStatus bloodMoonStatus) {
            if (!TimeLoopPolicy.CanScheduleHordeNightRewind(ConfigManager.Instance.Config, bloodMoonStatus))
                return false;

            if (_hordeRewindPendingAt != null)
                return true;

            _hordeRewindPendingAt = _gameWorldAdapter.GetUnscaledTime();
            Log.Out(TimeLoopText.WithPrefix("Horde-night rewind scheduled in {0} second(s).", HordeRewindGraceSeconds));
            MessageHelper.SendGlobalChat(TimeLoopText.WithPrefix(
                "Not enough players for horde night. Rewinding to the previous day in {0} second(s) unless conditions recover.",
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
                Log.Out(TimeLoopText.WithPrefix(
                    "Horde-night rewind not scheduled because the horde has already started or the day is not a scheduled blood moon."));
                ClearPendingHordeNightRewind(notifyPlayers: false);
                return;
            }

            if (_gameWorldAdapter.GetUnscaledTime() - _hordeRewindPendingAt.Value < HordeRewindGraceSeconds)
                return;

            _timesLooped = 0;
            _hordeRewindPendingAt = null;
            Log.Out(TimeLoopText.WithPrefix("Horde-night rewind executed."));
            MessageHelper.SendGlobalChat(TimeLoopText.WithPrefix(
                "Not enough players for horde night. Rewinding to the previous day."));
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
                            MessageHelper.SendGlobalChat("You seem to be stuck on the same day.");
                        break;
                    case true:
                        if (ConfigManager.Instance.Config.DaysToSkip > 0)
                            Log.Out(TimeLoopText.WithPrefix("Loop skipped. Resetting DaysToSkip to 0"));
                        ConfigManager.Instance.Config.DaysToSkip = 0;
                        ClearPendingHordeNightRewind(notifyPlayers: true);
                        ConfigManager.Instance.SaveToFile();
                        MessageHelper.SendGlobalChat("Time flows normally.");
                        break;
                }

                IsTimeFlowing = newState;
            }

            Log.Out(TimeLoopText.WithPrefix("Time flow status: {0}, days to skip: {1}",
                newState, ConfigManager.Instance.Config.DaysToSkip));
        }

        private void SkipLoop() {
            _timesLooped = 0;
            _gameWorldAdapter.AdvanceWorldTime(20UL);
            MessageHelper.SendGlobalChat(TimeLoopText.WithPrefix("Resetting day"));
            if (ConfigManager.Instance.DecreaseDaysToSkip() > 0)
                MessageHelper.SendGlobalChat(TimeLoopText.WithPrefix(
                    "The following {0} day(s) will NOT loop",
                    ConfigManager.Instance.Config.DaysToSkip));
            Log.Out(TimeLoopText.WithPrefix("Skipping the loop for today. Remaining: {0} days",
                ConfigManager.Instance.Config.DaysToSkip));
        }

        private void LoopDay() {
            if (IsDaySkippable()) {
                SkipLoop();
                return;
            }

            Log.Out(TimeLoopText.WithPrefix("Time reset."));
            MessageHelper.SendGlobalChat(TimeLoopText.WithPrefix("Resetting day"));
            var previousDay = GameUtils.WorldTimeToDays(_gameWorldAdapter.GetWorldTime()) - 1;
            _gameWorldAdapter.SetWorldTime(GameUtils.DaysToWorldTime(previousDay) + 20);
        }

        private void LimitedLoop() {
            if (!IsLoopLimitReached()) {
                LoopDay();
                _timesLooped++;
                Log.Out(TimeLoopText.WithPrefix("Loops: {0}/{1}", _timesLooped, ConfigManager.Instance.Config.LoopLimit));
                return;
            }

            Log.Out(TimeLoopText.WithPrefix("Loop limit reached."));
            MessageHelper.SendGlobalChat(TimeLoopText.WithPrefix("Loop limit reached."));
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
