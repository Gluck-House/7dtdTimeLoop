using System;
using System.Linq;
using System.Reflection;
using TimeLoop.Models;

namespace TimeLoop.Services {
    public sealed class BloodMoonService : IBloodMoonService {
        private static readonly Type? GamePrefsType = FindGameType("GamePrefs");
        private static readonly Type? EnumGamePrefsType = FindGameType("EnumGamePrefs");
        private static readonly Type? GameStatsType = FindGameType("GameStats");
        private static readonly Type? EnumGameStatsType = FindGameType("EnumGameStats");

        private readonly IGameWorldAdapter _gameWorldAdapter;

        public BloodMoonService(IGameWorldAdapter gameWorldAdapter) {
            _gameWorldAdapter = gameWorldAdapter;
        }

        public BloodMoonStatus GetStatus() {
            var currentDay = _gameWorldAdapter.GetCurrentDay();
            var dayTime = _gameWorldAdapter.GetDayTime();

            return new BloodMoonStatus(
                currentDay,
                dayTime,
                GetBloodMoonStartTime(),
                IsScheduledBloodMoonDay(currentDay));
        }

        private static Type? FindGameType(string typeName) {
            return typeof(GameManager).Assembly.GetTypes().FirstOrDefault(type => type.Name == typeName);
        }

        private static bool TryGetEnumValue(Type? enumType, string enumName, out object? value) {
            value = null;
            if (enumType == null || !enumType.IsEnum)
                return false;

            try {
                value = Enum.Parse(enumType, enumName);
                return true;
            }
            catch {
                return false;
            }
        }

        private static object? GetSingletonTarget(Type type) {
            var property = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (property != null)
                return property.GetValue(null);

            var field = type.GetField("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                        ?? type.GetField("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return field?.GetValue(null);
        }

        private static bool TryInvokeIntAccessor(Type? ownerType, Type? enumType, string enumName, out int value) {
            value = 0;
            if (ownerType == null || !TryGetEnumValue(enumType, enumName, out var enumValue))
                return false;

            var staticMethod = ownerType.GetMethod("GetInt",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new[] { enumType! },
                null);
            if (staticMethod?.Invoke(null, new[] { enumValue }) is int staticValue) {
                value = staticValue;
                return true;
            }

            var instanceTarget = GetSingletonTarget(ownerType);
            if (instanceTarget == null)
                return false;

            var instanceMethod = ownerType.GetMethod("GetInt",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { enumType! },
                null);
            if (instanceMethod?.Invoke(instanceTarget, new[] { enumValue }) is int instanceValue) {
                value = instanceValue;
                return true;
            }

            return false;
        }

        private static bool TryReadIntMember(Type? ownerType, string memberName, out int value) {
            value = 0;
            if (ownerType == null)
                return false;

            var property = ownerType.GetProperty(memberName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (property != null) {
                var target = property.GetMethod?.IsStatic == true ? null : GetSingletonTarget(ownerType);
                if (property.GetMethod?.IsStatic != true && target == null)
                    return false;
                if (property.GetValue(target) is int propertyValue) {
                    value = propertyValue;
                    return true;
                }
            }

            var field = ownerType.GetField(memberName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            if (field == null)
                return false;

            var fieldTarget = field.IsStatic ? null : GetSingletonTarget(ownerType);
            if (field.GetValue(fieldTarget) is int fieldValue) {
                value = fieldValue;
                return true;
            }

            return false;
        }

        private static int GetBloodMoonStartTime() {
            return 22000;
        }

        private static bool TryGetBloodMoonFrequency(out int frequency) {
            return TryInvokeIntAccessor(GamePrefsType, EnumGamePrefsType, "BloodMoonFrequency", out frequency);
        }

        private static bool TryGetBloodMoonRange(out int range) {
            return TryInvokeIntAccessor(GamePrefsType, EnumGamePrefsType, "BloodMoonRange", out range);
        }

        private static bool TryGetScheduledBloodMoonDay(out int scheduledDay) {
            if (TryReadIntMember(GameStatsType, "PropBloodMoonDay", out scheduledDay) && scheduledDay > 0)
                return true;

            if (TryReadIntMember(GameStatsType, "BloodMoonDay", out scheduledDay) && scheduledDay > 0)
                return true;

            if (TryInvokeIntAccessor(GameStatsType, EnumGameStatsType, "BloodMoonDay", out scheduledDay) && scheduledDay > 0)
                return true;

            scheduledDay = 0;
            return false;
        }

        private static bool IsScheduledBloodMoonDay(int currentDay) {
            if (TryGetScheduledBloodMoonDay(out var scheduledDay))
                return scheduledDay == currentDay;

            if (!TryGetBloodMoonFrequency(out var frequency) || frequency <= 0)
                return false;

            if (TryGetBloodMoonRange(out var range) && range > 0)
                return false;

            return currentDay % frequency == 0;
        }
    }
}
