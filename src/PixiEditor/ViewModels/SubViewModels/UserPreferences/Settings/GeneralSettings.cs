namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings;

public class GeneralSettings : SettingsGroup
{
    private bool imagePreviewInTaskbar = GetPreference(nameof(ImagePreviewInTaskbar), false);

    public bool ImagePreviewInTaskbar
    {
        get => imagePreviewInTaskbar;
        set => RaiseAndUpdatePreference(ref imagePreviewInTaskbar, value);
    }

    private bool isDebugModeEnabled = GetPreference(nameof(IsDebugModeEnabled), false);
    public bool IsDebugModeEnabled
    {
        get => isDebugModeEnabled;
        set => RaiseAndUpdatePreference(ref isDebugModeEnabled, value);
    }
}