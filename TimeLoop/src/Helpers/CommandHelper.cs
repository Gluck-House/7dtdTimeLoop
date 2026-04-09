using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TimeLoop.Managers;

namespace TimeLoop.Helpers {
    public static class CommandHelper {
        public static bool ValidateType<T>(string value, int paramIndex, out T output) {
            try {
                output = default!;
                var converted = TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(value);
                if (converted != null) output = (T)converted;
                return true;
            }
            catch (Exception e) {
#if DEBUG
                Log.Exception(e);
#endif
                SdtdConsole.Instance.Output(LocaleManager.Instance.LocalizeWithPrefix("cmd_invalid_param_type",
                    paramIndex,
                    typeof(T).Name, value));
                output = default!;
                return false;
            }
        }

        public static bool HasValue(string value, string[] array) {
            if (array.Contains(value)) return true;
            SdtdConsole.Instance.Output(
                LocaleManager.Instance.LocalizeWithPrefix("cmd_invalid_param", array.Join(), value)
            );
            return false;
        }

        public static bool ValidateCount(List<string> values, int requiredCount) {
            if (values.Count == requiredCount) return true;
            SdtdConsole.Instance.Output(
                LocaleManager.Instance.LocalizeWithPrefix("cmd_invalid_param_count", requiredCount, values.Count)
            );
            return false;
        }
    }
}
