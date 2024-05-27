namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

/// <summary>
/// A preference which will only be available on the current device
/// </summary>
/// <param name="name">The name of the preference</param>
/// <param name="fallbackValue">A optional fallback value which will be used if the setting has not been set before</param>
public class LocalSetting<T>(string name, T? fallbackValue = default) : Setting<T>(name, fallbackValue)
{
    protected override TAny? GetValue<TAny>(IPreferences preferences, TAny fallbackValue) where TAny : default =>
        preferences.GetLocalPreference(Name, fallbackValue);

    protected override void SetValue(IPreferences preferences, T value) =>
        preferences.UpdateLocalPreference(Name, value);
}
