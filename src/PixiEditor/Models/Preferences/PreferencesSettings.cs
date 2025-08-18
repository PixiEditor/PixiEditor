using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Preferences;

[DebuggerDisplay("{Preferences.Count + LocalPreferences.Count} Preference(s)")]
internal class PreferencesSettings : IPreferences
{
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
        name = TrimPrefix(name);

        if (IsLoaded == false)
        {
            Init();
        }

        Preferences[name] = value;

        if (Callbacks.TryGetValue(name, out var callback))
        {
            foreach (var action in callback)
            {
                action.Invoke(name, value);
            }
        }

        Save();
    }

    public void UpdateLocalPreference<T>(string name, T value)
    {
        name = TrimPrefix(name);

        if (IsLoaded == false)
        {
            Init();
        }

        LocalPreferences[name] = value;

        if (Callbacks.TryGetValue(name, out var callback))
        {
            foreach (var action in callback)
            {
                action.Invoke(name, value);
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

        try
        {
            File.WriteAllText(PathToRoamingUserPreferences, JsonConvert.SerializeObject(Preferences));
            File.WriteAllText(PathToLocalPreferences, JsonConvert.SerializeObject(LocalPreferences));
        }
        catch (Exception ex)
        {
            NoticeDialog.Show(
                new LocalizedString("ERROR_SAVING_PREFERENCES_DESC", ex.Message),
                "ERROR_SAVING_PREFERENCES");
        }
    }

    public Dictionary<string, List<Action<string, object>>> Callbacks { get; set; } = new Dictionary<string, List<Action<string, object>>>();

    public void AddCallback(string name, Action<string, object> action)
    {
        name = TrimPrefix(name);

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (Callbacks.ContainsKey(name))
        {
            Callbacks[name].Add(action);
            return;
        }

        Callbacks.Add(name, new List<Action<string, object>>() { action });
    }

    public void AddCallback<T>(string name, Action<string, T> action)
    {
        name = TrimPrefix(name);

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        AddCallback(name, new Action<string, object>((n, o) => action(n, (T)o)));
    }

    public void RemoveCallback(string name, Action<string, object> action)
    {
        name = TrimPrefix(name);

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (Callbacks.TryGetValue(name, out var callback))
        {
            callback.Remove(action);
        }
    }

    public void RemoveCallback<T>(string name, Action<string, T> action)
    {
        name = TrimPrefix(name);

        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        RemoveCallback(name, new Action<string, object>((n, o) => action(n, (T)o)));
    }

#nullable enable

    public T? GetPreference<T>(string name)
    {
        name = TrimPrefix(name);

        return GetPreference(name, default(T));
    }

    public T? GetPreference<T>(string name, T? fallbackValue)
    {
        name = TrimPrefix(name);

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
        name = TrimPrefix(name);

        return GetLocalPreference(name, default(T));
    }

    public T? GetLocalPreference<T>(string name, T? fallbackValue)
    {
        name = TrimPrefix(name);
        
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
        name = TrimPrefix(name);
        
        if (!dict.ContainsKey(name)) return fallbackValue;
        var preference = dict[name];
        if (typeof(T) == preference.GetType()) return (T)preference;

        if (typeof(T).IsEnum)
        {
            return (T)Enum.Parse(typeof(T), preference.ToString());
        }
        
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

    private const string Prefix = "PixiEditor:";

    private string TrimPrefix(string value) => value.StartsWith("PixiEditor:") ? value[Prefix.Length..] : value;
}
