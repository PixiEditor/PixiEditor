namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings;

internal class ToolsSettings : SettingsGroup
{
    private bool enableSharedToolbar = GetPreference(nameof(EnableSharedToolbar), false);

    public bool EnableSharedToolbar
    {
        get => enableSharedToolbar;
        set
        {
            enableSharedToolbar = value;
            RaiseAndUpdatePreference(nameof(EnableSharedToolbar), value);
        }
    }
}
