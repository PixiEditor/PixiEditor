namespace PixiEditor.ViewModels.UserPreferences.Settings;

internal class AccessibilitySettings : SettingsGroup
{
    
    private double _uiScaleFactor = GetPreference(nameof(UiScaleFactor), 1.0);
    
    public double UiScaleFactor
    {
        get => _uiScaleFactor;
        set
        {
            RaiseAndUpdatePreference(ref _uiScaleFactor, value);
            OnPropertyChanged(nameof(UiScaleFactorPreview));
        }
    }

    public string UiScaleFactorPreview => (UiScaleFactor * 100).ToString("F0") + "%";
}
