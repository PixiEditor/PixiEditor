using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixiEditor.Extensions.Common.UserPreferences;

namespace PixiEditor.Models.Preferences;

[DebuggerDisplay("{Preferences.Count + LocalPreferences.Count} Preference(s)")]
internal class PreferencesSettings : IPreferences
{
    public static IPreferences Current => ViewModelMain.Current.Preferences;

    public bool IsLoaded { get; private set; } = false;

    public string PathToRoamingUserPreferences { get; private set; } = GetPathToSettings(Environment.SpecialFolder.ApplicationData, "user_preferences.json");

    public string PathToLocalPreferences { get; private set; } = GetPathToSettings(Environment.SpecialFolder.LocalApplicationData, "editor_data.json");

    public Dictionary<string, object> Preferences { get; set; } = new Dictionary<string, object>();

    public Dictionary<string, object> LocalPreferences { get; set; } = new Dictionary<string, object>();

    public void Init()
    {
        IPreferences.SetAsCurrent(this);
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

    public void AddCallback(string name, Action<object> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (Callbacks.ContainsKey(name))
        {
            Callbacks[name].Add(action);
            return;
        }

        Callbacks.Add(name, new List<Action<object>>() { action });
    }

    public void AddCallback<T>(string name, Action<T> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        AddCallback(name, new Action<object>(o => action((T)o)));
    }

    public void RemoveCallback(string name, Action<object> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (Callbacks.TryGetValue(name, out var callback))
        {
            callback.Remove(action);
        }
    }

    public void RemoveCallback<T>(string name, Action<T> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        RemoveCallback(name, new Action<object>(o => action((T)o)));
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

        try
        {
            return GetValue(Preferences, name, fallbackValue);
        }
        catch (InvalidCastException)
        {
            Preferences.Remove(name);
            Save();

            return fallbackValue;
        }
    }

    public T? GetLocalPreference<T>(string name)
    {
        return GetLocalPreference(name, default(T));
    }

    public T? GetLocalPreference<T>(string name, T? fallbackValue)
    {
        if (IsLoaded == false)
        {
            Init();
        }

        try
        {
            return GetValue(LocalPreferences, name, fallbackValue);
        }
        catch (InvalidCastException)
        {
            LocalPreferences.Remove(name);
            Save();

            return fallbackValue;
        }
    }

    private T? GetValue<T>(Dictionary<string, object> dict, string name, T? fallbackValue)
    {
        if (!dict.ContainsKey(name)) return fallbackValue;
        var preference = dict[name];
        if (typeof(T) == preference.GetType()) return (T)preference;
        if (preference.GetType() == typeof(JArray))
        {
            return ((JArray)preference).ToObject<T>();
        }

        return (T)Convert.ChangeType(dict[name], typeof(T));
    }

#nullable disable

    private static string GetPathToSettings(Environment.SpecialFolder folder, string fileName)
    {
        return Path.Join(
            Environment.GetFolderPath(folder),
            "PixiEditor",
            fileName);
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
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            // dictionary is null if the user deletes the content of the preference file.
            if (dictionary != null)
            {
                return dictionary;
            }
        }

        return new Dictionary<string, object>();
    }
}
