using System;
using System.Collections.Generic;
using TimeLoop.Helpers;
using TimeLoop.Managers;

namespace TimeLoop.Modules.ConsoleCommands {
    public class LoopingStateCommand : TimeLoopConsoleCommandBase {
        protected override string GetHelpText() {
            return "Usage:\ntl_state\n    Displays the current loop state and pending horde-night rewind status.";
        }

        public override string[] getCommands() {
            return new[] { "tl_state", "timeloop_state" };
        }

        public override string getDescription() {
            return "Displays if the current day will loop or not.";
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
            var status = TimeLoopManager.Instance.GetStatus();
            var isOrNot = status.IsTimeFlowing ? "is not" : "is";
            var loopLimitText = status.LoopLimit > 0
                ? status.LoopLimit.ToString()
                : "infinite";
            var hordeStatus = status.IsHordeRewindPending
                ? string.Format("pending in {0} second(s)", status.PendingHordeRewindSeconds)
                : "not pending";

            var output = string.Join(
                Environment.NewLine,
                TimeLoopText.WithPrefix("Current day {0} looping", isOrNot),
                TimeLoopText.WithPrefix("Current mode: {0}", TimeLoopText.ModeName(status.Mode)),
                TimeLoopText.WithPrefix("Players online: {0} | Authorized players online: {1}",
                    status.PlayerActivity.ConnectedPlayers,
                    status.PlayerActivity.AuthorizedPlayers),
                TimeLoopText.WithPrefix("Loops this day: {0}/{1}",
                    status.TimesLooped,
                    loopLimitText),
                TimeLoopText.WithPrefix("Days to skip remaining: {0}", status.DaysToSkip),
                TimeLoopText.WithPrefix("Blood moon day {0}: {1}",
                    status.BloodMoonStatus.CurrentDay,
                    status.BloodMoonStatus.IsScheduledBloodMoonDay
                        ? "enabled"
                        : "disabled"),
                TimeLoopText.WithPrefix("Horde-night rewind: {0}", hordeStatus));
            SdtdConsole.Instance.Output(output);
        }
    }
}
