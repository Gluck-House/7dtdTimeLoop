using System;
using System.Reflection;
using UnityEngine;

namespace TimeLoop.Services {
    public sealed class GameWorldAdapter : IGameWorldAdapter {
        public ulong GetWorldTime() {
            return GameManager.Instance.World.GetWorldTime();
        }

        public void SetWorldTime(ulong worldTime) {
            GameManager.Instance.World.SetTime(worldTime);
        }

        public void AdvanceWorldTime(ulong delta) {
            GameManager.Instance.World.worldTime += delta;
        }

        public int GetCurrentDay() {
            var world = GameManager.Instance.World;
            var worldType = world.GetType();

            var getCurrentDay = worldType.GetMethod("GetCurrentDay", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (getCurrentDay?.Invoke(world, null) is int currentDay)
                return currentDay;

            var currentDayCount = worldType.GetProperty("CurrentDayCount",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (currentDayCount?.GetValue(world) is int currentDayCountValue)
                return currentDayCountValue;

            return Math.Max(1, GameUtils.WorldTimeToDays(GetWorldTime()) - 1);
        }

        public int GetDayTime() {
            return (int)(GetWorldTime() % 24000);
        }

        public double GetUnscaledTime() {
            return Time.unscaledTimeAsDouble;
        }
    }
}
