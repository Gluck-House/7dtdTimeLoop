using System;
using System.Collections.Generic;
using TimeLoop.Enums;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class LoopingStateCommand : TimeLoopConsoleCommandBase {
        private static string GetModeLocaleKey(EMode mode) {
            return mode switch {
                EMode.Always => "always",
                EMode.Whitelist => "whitelist",
                EMode.Threshold => "threshold",
                EMode.WhitelistedThreshold => "whitelisted_threshold",
                _ => "cmd_mode_invalid_mode"
            };
        }

        protected override string GetHelpText() {
            return LocaleManager.Instance.Localize("cmd_loopstate_help");
        }

        public override string[] getCommands() {
            return new[] { "tl_state", "timeloop_state" };
        }

        public override string getDescription() {
            return LocaleManager.Instance.LocalizeWithPrefix("cmd_loopstate_desc");
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            var status = TimeLoopManager.Instance.GetStatus();
            var isOrNot = status.IsTimeFlowing ? "is_not" : "is";
            var loopLimitText = status.LoopLimit > 0
                ? status.LoopLimit.ToString()
                : LocaleManager.Instance.Localize("infinite");
            var hordeStatus = status.IsHordeRewindPending
                ? LocaleManager.Instance.Localize("cmd_loopstate_horde_pending", status.PendingHordeRewindSeconds)
                : LocaleManager.Instance.Localize("cmd_loopstate_horde_none");

            var output = string.Join(
                Environment.NewLine,
                LocaleManager.Instance.LocalizeWithPrefix("cmd_loopstate_return",
                    LocaleManager.Instance.Localize(isOrNot)),
                LocaleManager.Instance.LocalizeWithPrefix("cmd_loopstate_mode",
                    LocaleManager.Instance.Localize(GetModeLocaleKey(status.Mode))),
                LocaleManager.Instance.LocalizeWithPrefix("cmd_loopstate_players",
                    status.PlayerActivity.ConnectedPlayers,
                    status.PlayerActivity.AuthorizedPlayers),
                LocaleManager.Instance.LocalizeWithPrefix("cmd_loopstate_loops",
                    status.TimesLooped,
                    loopLimitText),
                LocaleManager.Instance.LocalizeWithPrefix("cmd_loopstate_skipdays",
                    status.DaysToSkip),
                LocaleManager.Instance.LocalizeWithPrefix("cmd_loopstate_bloodmoon",
                    status.BloodMoonStatus.CurrentDay,
                    status.BloodMoonStatus.IsScheduledBloodMoonDay
                        ? LocaleManager.Instance.Localize("enabled")
                        : LocaleManager.Instance.Localize("disabled")),
                LocaleManager.Instance.LocalizeWithPrefix("cmd_loopstate_horde", hordeStatus));
            SdtdConsole.Instance.Output(output);
        }
    }
}
