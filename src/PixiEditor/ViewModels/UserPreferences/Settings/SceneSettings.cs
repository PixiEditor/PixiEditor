using Drawie.Numerics;
using PixiEditor.Extensions.CommonApi.UserPreferences;

namespace PixiEditor.ViewModels.UserPreferences.Settings;

internal class SceneSettings : SettingsGroup
{
    private bool autoScaleBackground = GetPreference(PreferencesConstants.AutoScaleBackground, PreferencesConstants.AutoScaleBackgroundDefault);
    public bool AutoScaleBackground
    {
        get => autoScaleBackground;
        set => RaiseAndUpdatePreference(ref autoScaleBackground, value);
    }

    private double customBackgroundScaleX = GetPreference(PreferencesConstants.CustomBackgroundScaleX, PreferencesConstants.CustomBackgroundScaleDefault);
    public double CustomBackgroundScaleX
    {
        get => customBackgroundScaleX;
        set => RaiseAndUpdatePreference(ref customBackgroundScaleX, value);
    }

    private double customBackgroundScaleY = GetPreference(PreferencesConstants.CustomBackgroundScaleY, PreferencesConstants.CustomBackgroundScaleDefault);
    public double CustomBackgroundScaleY
    {
        get => customBackgroundScaleY;
        set => RaiseAndUpdatePreference(ref customBackgroundScaleY, value);
    }
}
