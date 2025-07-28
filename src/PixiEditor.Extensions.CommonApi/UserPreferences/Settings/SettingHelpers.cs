using System.Diagnostics;

namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

public static class SettingHelper
{
    public static List<T> AsList<T>(this Setting<IEnumerable<T>> setting) =>
        setting.As(new List<T>());
    
    public static T[] AsArray<T>(this Setting<IEnumerable<T>> setting) =>
        setting.As(Array.Empty<T>());

    public static void AddListCallback<T>(this Setting<IEnumerable<T>> setting, Action<List<T>> callback) =>
        setting.ValueChanged += (_, value) => callback(value.ToList());

    public static void AddArrayCallback<T>(this Setting<IEnumerable<T>> setting, Action<T[]> callback) =>
        setting.ValueChanged += (_, value) => callback(value.ToArray());
    
    [StackTraceHidden]
    public static void ThrowIfEmptySettingName(string name)
    {
        if (string.IsNullOrEmpty(name)) {
            throw new ArgumentException($"name was empty", nameof(name));
        }
    
        var colon = name.IndexOf(':');
    
        if (colon == 0)
        {
            // ":<any key>" does not have a valid prefix
            throw new ArgumentException($"The prefix in the name '{name}' was empty", nameof(name));
        }

        if (colon == name.Length - 1)
        {
            // "<any prefix>:" does not have a valid key
            throw new ArgumentException($"The key in the name '{name}' was empty", nameof(name));
        }
    }
}
