using System.Diagnostics;
using System.Text.Json;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Models.Dialogs;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.Preferences;

[DebuggerDisplay("{Preferences.Count + LocalPreferences.Count} Preference(s)")]
internal class PreferencesSettings : IPreferences
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public bool IsLoaded { get; private set; } = false;

    public string PathToRoamingUserPreferences { get; private set; } = GetPathToSettings(Environment.SpecialFolder.ApplicationData, "user_preferences.json");
    public string PathToLocalPreferences { get; private set; } = GetPathToSettings(Environment.SpecialFolder.LocalApplicationData, "editor_data.json");

    public Dictionary<string, object> Preferences { get; set; } = new();
    public Dictionary<string, object> LocalPreferences { get; set; } = new();

    public void Init()
    {
        IPreferences.SetAsCurrent(this);
        Init(PathToRoamingUserPreferences, PathToLocalPreferences);
    }

    public void Init(string path, string localPath)
    {
        PathToRoamingUserPreferences = path;
        PathToLocalPreferences = localPath;

        if (!IsLoaded)
        {
            Preferences = InitPath(path);
            LocalPreferences = InitPath(localPath);
            IsLoaded = true;
        }
    }

    public void UpdatePreference<T>(string name, T value) => UpdateGeneric(Preferences, name, value);
    public void UpdateLocalPreference<T>(string name, T value) => UpdateGeneric(LocalPreferences, name, value);

    private void UpdateGeneric<T>(Dictionary<string, object> dict, string name, T value)
    {
        name = TrimPrefix(name);
        if (!IsLoaded) Init();

        dict[name] = value;

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
        if (!IsLoaded) Init();

        try
        {
            File.WriteAllText(PathToRoamingUserPreferences, JsonSerializer.Serialize(Preferences, JsonOptions));
            File.WriteAllText(PathToLocalPreferences, JsonSerializer.Serialize(LocalPreferences, JsonOptions));
        }
        catch (Exception ex)
        {
            NoticeDialog.Show(
                new LocalizedString("ERROR_SAVING_PREFERENCES_DESC", ex.Message),
                "ERROR_SAVING_PREFERENCES");
        }
    }

    public Dictionary<string, List<Action<string, object>>> Callbacks { get; set; } = new();

    // ... (AddCallback / RemoveCallback methods remain largely the same, logic doesn't change)
    public void AddCallback(string name, Action<string, object> action)
    {
        name = TrimPrefix(name);
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (!Callbacks.ContainsKey(name)) Callbacks[name] = new List<Action<string, object>>();
        Callbacks[name].Add(action);
    }

    public void AddCallback<T>(string name, Action<string, T> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        AddCallback(name, (n, o) => action(n, (T)o));
    }

    public void RemoveCallback(string name, Action<string, object> action)
    {
        name = TrimPrefix(name);
        if (Callbacks.TryGetValue(name, out var callback)) callback.Remove(action);
    }

    public void RemoveCallback<T>(string name, Action<string, T> action)
    {
        RemoveCallback(name, (n, o) => action(n, (T)o));
    }

#nullable enable

    public T? GetPreference<T>(string name, T? fallbackValue = default)
    {
        if (!IsLoaded) Init();
        return TryGetFromDict(Preferences, name, fallbackValue);
    }

    public T? GetLocalPreference<T>(string name, T? fallbackValue = default)
    {
        if (!IsLoaded) Init();
        return TryGetFromDict(LocalPreferences, name, fallbackValue);
    }

    private T? TryGetFromDict<T>(Dictionary<string, object> dict, string name, T? fallbackValue)
    {
        name = TrimPrefix(name);
        try
        {
            return GetValue(dict, name, fallbackValue);
        }
        catch (Exception) // Catching more broadly for serialization failures
        {
            dict.Remove(name);
            Save();
            return fallbackValue;
        }
    }

    private T? GetValue<T>(Dictionary<string, object> dict, string name, T? fallbackValue)
    {
        if (!dict.TryGetValue(name, out var preference)) return fallbackValue;

        // If it's already the right type (happens for values set during current session)
        if (preference is T correctlyTypedValue) return correctlyTypedValue;

        // System.Text.Json deserializes 'object' values as JsonElement
        if (preference is JsonElement element)
        {
            // This handles Enums, Arrays, Custom Classes, and Primitives
            return element.Deserialize<T>(JsonOptions);
        }

        // Fallback for types that might have been changed manually in the dictionary
        return (T)Convert.ChangeType(preference, typeof(T));
    }

#nullable disable

    private static Dictionary<string, object> InitPath(string path)
    {
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "{}");
            return new Dictionary<string, object>();
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions) 
                   ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private static string GetPathToSettings(Environment.SpecialFolder folder, string fileName) =>
        Path.Combine(Environment.GetFolderPath(folder), "PixiEditor", fileName);

    private const string Prefix = "PixiEditor:";
    private string TrimPrefix(string value) => value.StartsWith(Prefix) ? value[Prefix.Length..] : value;
}
