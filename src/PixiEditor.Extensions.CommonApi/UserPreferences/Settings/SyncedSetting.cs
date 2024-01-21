namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

public class SyncedSetting<T>(string name, T? fallbackValue = default) : Setting<T>(name, fallbackValue)
{
    protected override TAny? GetValue<TAny>(IPreferences preferences, TAny fallbackValue) where TAny : default =>
        preferences.GetPreference(Name, fallbackValue);

    protected override void SetValue(IPreferences preferences, T value) =>
        preferences.UpdatePreference(Name, value);
}
