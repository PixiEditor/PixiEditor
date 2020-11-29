using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection.Metadata;
using Newtonsoft.Json;

namespace PixiEditor.Models.UserPreferences
{
    public static class PreferencesSettings
    {
        public static bool IsLoaded { get; private set; } = false;

        public static string PathToUserPreferences { get; } = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PixiEditor",
            "user_preferences.json");

        public static Dictionary<string, object> Preferences { get; set; } = new Dictionary<string, object>();

        public static void Init()
        {
            if (IsLoaded == false)
            {
                string dir = Path.GetDirectoryName(PathToUserPreferences);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(PathToUserPreferences))
                {
                    File.WriteAllText(PathToUserPreferences, "{\n}");
                }
                else
                {
                    string json = File.ReadAllText(PathToUserPreferences);
                    Preferences = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                }

                IsLoaded = true;
            }
        }

        public static void UpdatePreference(string name, object value)
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