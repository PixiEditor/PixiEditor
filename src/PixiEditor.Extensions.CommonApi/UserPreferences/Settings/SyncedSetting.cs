using System.Runtime.CompilerServices;

namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

/// <summary>
/// A static class for creating a LocalSetting from the property name
/// </summary>
public static class SyncedSetting
{
    public static SyncedSetting<T> Owned<T>(T? fallbackValue = default, [CallerMemberName] string name = "") =>
        new(name, fallbackValue);
    
    public static SyncedSetting<T> NonOwned<T>(string prefix, T? fallbackValue = default, [CallerMemberName] string name = "") =>
        new($"{prefix}:{name}", fallbackValue);
}

/// <summary>
/// A preference which may be synced across multiple devices
/// </summary>
/// <param name="name">The name of the preference</param>
/// <param name="fallbackValue">A optional fallback value which will be used if the setting has not been set before set before</param>
public class SyncedSetting<T>(string name, T? fallbackValue = default) : Setting<T>(name, fallbackValue)
{
    protected override TAny? GetValue<TAny>(IPreferences preferences, TAny fallbackValue) where TAny : default =>
        preferences.GetPreference(Name, fallbackValue);

    protected override void SetValue(IPreferences preferences, T value) =>
        preferences.UpdatePreference(Name, value);
}
