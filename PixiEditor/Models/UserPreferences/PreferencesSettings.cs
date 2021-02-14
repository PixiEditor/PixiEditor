using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.UserPreferences
{
    public class PreferencesSettings : IPreferences
    {
        public static IPreferences Current => ViewModelMain.Current.Preferences;

        public bool IsLoaded { get; private set; } = false;

        public string PathToRoamingUserPreferences { get; private set; } = GetPathToSettings(Environment.SpecialFolder.ApplicationData);

        public string PathToLocalPreferences { get; private set; } = GetPathToSettings(Environment.SpecialFolder.LocalApplicationData);

        public Dictionary<string, object> Preferences { get; set; } = new Dictionary<string, object>();

        public Dictionary<string, object> LocalPreferences { get; set; } = new Dictionary<string, object>();

        public void Init()
        {
            Init(PathToRoamingUserPreferences, PathToLocalPreferences);
        }

        public void Init(string path, string localPath)
        {
            PathToRoamingUserPreferences = path;
            PathToLocalPreferences = localPath;

            if (IsLoaded == false)
            {
                Preferences = InitPath(path);
                LocalPreferences = InitPath(localPath);

                IsLoaded = true;
            }
        }

        public void UpdatePreference<T>(string name, T value)
        {
            if (IsLoaded == false)
            {
                Init();
            }

            Preferences[name] = value;

            if (Callbacks.ContainsKey(name))
            {
                foreach (var action in Callbacks[name])
                {
                    action.Invoke(value);
                }
            }

            Save();
        }

        public void UpdateLocalPreference<T>(string name, T value)
        {
            if (IsLoaded == false)
            {
                Init();
            }

            LocalPreferences[name] = value;

            if (Callbacks.ContainsKey(name))
            {
                foreach (var action in Callbacks[name])
                {
                    action.Invoke(value);
                }
            }

            Save();
        }

        public void Save()
        {
            if (IsLoaded == false)
            {
                Init();
            }

            File.WriteAllText(PathToRoamingUserPreferences, JsonConvert.SerializeObject(Preferences));
            File.WriteAllText(PathToLocalPreferences, JsonConvert.SerializeObject(LocalPreferences));
        }

        public Dictionary<string, List<Action<object>>> Callbacks { get; set; } = new Dictionary<string, List<Action<object>>>();

        public void AddCallback(string setting, Action<object> action)
        {
            if (Callbacks.ContainsKey(setting))
            {
                Callbacks[setting].Add(action);
                return;
            }

            Callbacks.Add(setting, new List<Action<object>>() { action });
        }

#nullable enable

        public T? GetPreference<T>(string name)
        {
            return GetPreference(name, default(T));
        }

        public T? GetPreference<T>(string name, T? fallbackValue)
        {
            if (IsLoaded == false)
            {
                Init();
            }

            return Preferences.ContainsKey(name)
                ? (T)Preferences[name]
                : fallbackValue;
        }

        public T? GetLocalPreference<T>(string name)
        {
            return GetPreference(name, default(T));
        }

        public T? GetLocalPreference<T>(string name, T? fallbackValue)
        {
            if (IsLoaded == false)
            {
                Init();
            }

            return LocalPreferences.ContainsKey(name)
                ? (T)LocalPreferences[name]
                : fallbackValue;
        }

#nullable disable

        private static string GetPathToSettings(Environment.SpecialFolder folder)
        {
            return Path.Join(
            Environment.GetFolderPath(folder),
            "PixiEditor",
            "user_preferences.json");
        }

        private static Dictionary<string, object> InitPath(string path)
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
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }

            return new Dictionary<string, object>();
        }
    }
}