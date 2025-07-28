namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

public delegate void SettingChangedHandler<T>(Setting<T> setting, T? newValue);
