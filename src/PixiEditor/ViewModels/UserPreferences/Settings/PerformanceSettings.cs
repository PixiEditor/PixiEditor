using PixiEditor.Extensions.CommonApi.UserPreferences;

namespace PixiEditor.ViewModels.UserPreferences.Settings;

internal class PerformanceSettings : SettingsGroup
{
    private bool disablePreviews = GetPreference(PreferencesConstants.DisablePreviews, PreferencesConstants.DisablePreviewsDefault);
    private int maxBilinearSize = GetPreference(PreferencesConstants.MaxBilinearSampleSize, PreferencesConstants.MaxBilinearSampleSizeDefault);

    public bool DisablePreviews
    {
        get => disablePreviews;
        set => RaiseAndUpdatePreference(ref disablePreviews, value, PreferencesConstants.DisablePreviews);
    }

    public int MaxBilinearSize
    {
        get => maxBilinearSize;
        set => RaiseAndUpdatePreference(ref maxBilinearSize, value, PreferencesConstants.MaxBilinearSampleSize);
    }
}
