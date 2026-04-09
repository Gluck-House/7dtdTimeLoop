using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TimeLoop.Managers {
    public class LocaleManager {
        private bool _isFallbackMode;
        private Dictionary<string, string> _localeDict;
        public List<string> LocaleList;

        private LocaleManager(string locale) {
            _localeDict = LoadLocale(locale);
            LocaleList = GetLocales();
        }

        public string LoadedLocale { get; private set; } = null!;

        private string GetLocalePath(string locale) {
            return Main.GetAbsolutePath(Path.Combine(Main.LocaleFolderPath, locale + ".json"));
        }

        public void SetLocale(string newLocale) {
            _localeDict = LoadLocale(newLocale);
        }

        private List<string> GetLocales() {
            try {
                var localePath = Main.GetAbsolutePath(Main.LocaleFolderPath);
                var locales = Directory.GetFiles(localePath, "*.json", SearchOption.TopDirectoryOnly);
                return locales.Select(Path.GetFileNameWithoutExtension).ToList();
            }
            catch (Exception e) {
                Log.Error("[TimeLoop] Failed to get all locale files. {0}", e.Message);
#if DEBUG
                Log.Exception(e);
#endif
                return new List<string>();
            }
        }

        private Dictionary<string, string> LoadLocale(string locale) {
            try {
                var localePath = TryGetValidLocale(locale) ?? TryGetValidLocale("en_us");
                if (localePath == null) {
                    Log.Error(
                        "[TimeLoop] Failed to load any locale file. Using raw keys. (Use tl_locale or review the config file)");
                    _isFallbackMode = true;
                    LoadedLocale = "invalid_locale";
                    return new Dictionary<string, string>();
                }

                LoadedLocale = Path.GetFileNameWithoutExtension(localePath);
                using var stream = new StreamReader(localePath);
                var localeDictionary = ParseLocaleDictionary(stream.ReadToEnd());
                _isFallbackMode = false;
                return localeDictionary;
            }
            catch (Exception e) {
                Log.Error("[TimeLoop] Failed to load localization file. {0}", e.Message);
#if DEBUG
                Log.Exception(e);
#endif
                _isFallbackMode = true;
                return new Dictionary<string, string>();
            }
        }

        private string? TryGetValidLocale(string locale) {
            var path = GetLocalePath(locale);
            if (File.Exists(path)) return path;
            Log.Error("[TimeLoop] Failed to load locale file for '{0}'", locale);
            return null;
        }

        private static Dictionary<string, string> ParseLocaleDictionary(string json) {
            var localeDict = new Dictionary<string, string>();
            var trimmed = json.Trim().TrimStart('\uFEFF');
            var index = 0;

            SkipWhitespace(trimmed, ref index);
            if (index >= trimmed.Length || trimmed[index] != '{')
                throw new FormatException("Locale file root must be a JSON object.");

            index++;
            while (true) {
                SkipWhitespace(trimmed, ref index);
                if (index >= trimmed.Length)
                    throw new FormatException("Locale file root must be a JSON object.");

                if (trimmed[index] == '}') {
                    index++;
                    break;
                }

                var key = ReadJsonString(trimmed, ref index);
                SkipWhitespace(trimmed, ref index);
                if (index >= trimmed.Length || trimmed[index] != ':')
                    throw new FormatException("Locale file root must be a JSON object.");

                index++;
                SkipWhitespace(trimmed, ref index);
                if (index >= trimmed.Length)
                    throw new FormatException("Locale file root must be a JSON object.");

                if (trimmed[index] == '"')
                    localeDict[key] = ReadJsonString(trimmed, ref index);
                else
                    SkipJsonValue(trimmed, ref index);

                SkipWhitespace(trimmed, ref index);
                if (index >= trimmed.Length)
                    throw new FormatException("Locale file root must be a JSON object.");

                if (trimmed[index] == ',') {
                    index++;
                    continue;
                }

                if (trimmed[index] == '}') {
                    index++;
                    break;
                }

                throw new FormatException("Locale file root must be a JSON object.");
            }

            if (localeDict.Count == 0)
                throw new FormatException("Locale file does not contain any string entries.");

            return localeDict;
        }

        private static void SkipWhitespace(string text, ref int index) {
            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;
        }

        private static string ReadJsonString(string text, ref int index) {
            if (index >= text.Length || text[index] != '"')
                throw new FormatException("Locale file root must be a JSON object.");

            index++;
            var value = new List<char>();
            while (index < text.Length) {
                var current = text[index++];
                if (current == '"')
                    return new string(value.ToArray());

                if (current != '\\') {
                    value.Add(current);
                    continue;
                }

                if (index >= text.Length)
                    throw new FormatException("Locale file root must be a JSON object.");

                value.Add(ReadEscapedCharacter(text, ref index));
            }

            throw new FormatException("Locale file root must be a JSON object.");
        }

        private static char ReadEscapedCharacter(string text, ref int index) {
            var escaped = text[index++];
            return escaped switch {
                '"' => '"',
                '\\' => '\\',
                '/' => '/',
                'b' => '\b',
                'f' => '\f',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                'u' => ReadUnicodeEscape(text, ref index),
                _ => escaped
            };
        }

        private static char ReadUnicodeEscape(string text, ref int index) {
            if (index + 4 > text.Length)
                throw new FormatException("Locale file root must be a JSON object.");

            var hex = text.Substring(index, 4);
            index += 4;
            return (char)Convert.ToInt32(hex, 16);
        }

        private static void SkipJsonValue(string text, ref int index) {
            switch (text[index]) {
                case '{':
                    SkipJsonObject(text, ref index);
                    return;
                case '[':
                    SkipJsonArray(text, ref index);
                    return;
                case '"':
                    ReadJsonString(text, ref index);
                    return;
                default:
                    while (index < text.Length && !IsValueTerminator(text[index]))
                        index++;
                    return;
            }
        }

        private static void SkipJsonObject(string text, ref int index) {
            var depth = 0;
            while (index < text.Length) {
                var current = text[index++];
                if (current == '"') {
                    index--;
                    ReadJsonString(text, ref index);
                    continue;
                }

                if (current == '{')
                    depth++;
                else if (current == '}') {
                    depth--;
                    if (depth == 0)
                        return;
                }
            }

            throw new FormatException("Locale file root must be a JSON object.");
        }

        private static void SkipJsonArray(string text, ref int index) {
            var depth = 0;
            while (index < text.Length) {
                var current = text[index++];
                if (current == '"') {
                    index--;
                    ReadJsonString(text, ref index);
                    continue;
                }

                if (current == '[')
                    depth++;
                else if (current == ']') {
                    depth--;
                    if (depth == 0)
                        return;
                } else if (current == '{') {
                    index--;
                    SkipJsonObject(text, ref index);
                }
            }

            throw new FormatException("Locale file root must be a JSON object.");
        }

        private static bool IsValueTerminator(char current) {
            return current == ',' || current == '}' || current == ']' || char.IsWhiteSpace(current);
        }

        public string Localize(string key) {
            if (_isFallbackMode || !_localeDict.TryGetValue(key, out var locale)) return key;

            return locale;
        }

        public string Localize(string key, params object[] args) {
            return string.Format(Localize(key), args);
        }

        public string LocalizeWithPrefix(string key) {
            return string.Concat(Localize("prefix"), Localize(key));
        }

        public string LocalizeWithPrefix(string key, params object[] args) {
            return string.Concat(Localize("prefix"), Localize(key, args));
        }

        #region Singleton

        private static LocaleManager? _instance;

        public static LocaleManager Instance {
            get { return _instance ??= new LocaleManager(ConfigManager.Instance.Config.Language); }
        }

        public static void Instantiate(string locale) {
            _instance = new LocaleManager(locale);
        }

        #endregion
    }
}
