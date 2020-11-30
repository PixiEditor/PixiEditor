using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace PixiEditor.Models.UserPreferences
{
    public static class PreferencesSettings
    {
        public static bool IsLoaded { get; private set; } = false;

        public static string PathToUserPreferences { get; private set; } = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PixiEditor",
            "user_preferences.json");

        public static Dictionary<string, object> Preferences { get; set; } = new Dictionary<string, object>();

        public static void Init()
        {
            Init(PathToUserPreferences);
        }

        public static void Init(string path)
        {
            PathToUserPreferences = path;
            if (IsLoaded == false)
            {
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(path))
                {
                    File.WriteAllText(path, "{\n}");
                }
                else
                {
                    string json = File.ReadAllText(path);
                    Preferences = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                }

                IsLoaded = true;
            }
        }

        public static void UpdatePreference<T>(string name, T value)
        {
            if (IsLoaded == false)
            {
                Init();
            }

            Preferences[name] = value;

            Save();
        }

        public static void Save()
        {
            if (IsLoaded == false)
            {
                Init();
            }

            File.WriteAllText(PathToUserPreferences, JsonConvert.SerializeObject(Preferences));
        }

#nullable enable

        public static T? GetPreference<T>(string name)
        {
            return GetPreference(name, default(T));
        }

        public static T? GetPreference<T>(string name, T? fallbackValue)
        {
            if (IsLoaded == false)
            {
                Init();
            }

            return Preferences.ContainsKey(name)
                ? (T)Preferences[name]
                : fallbackValue;
        }
    }
}