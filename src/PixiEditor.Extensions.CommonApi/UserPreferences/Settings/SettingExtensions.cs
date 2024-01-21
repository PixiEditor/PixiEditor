namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

public static class SettingExtensions
{
    public static List<T> AsList<T>(this Setting<IEnumerable<T>> setting) =>
        setting.As(new List<T>());
    
    public static T[] AsArray<T>(this Setting<IEnumerable<T>> setting) =>
        setting.As(Array.Empty<T>());

    public static void AddListCallback<T>(this Setting<IEnumerable<T>> setting, Action<List<T>> callback) =>
        setting.ValueChanged += (_, value) => callback(value.ToList());

    public static void AddArrayCallback<T>(this Setting<IEnumerable<T>> setting, Action<T[]> callback) =>
        setting.ValueChanged += (_, value) => callback(value.ToArray());
}
