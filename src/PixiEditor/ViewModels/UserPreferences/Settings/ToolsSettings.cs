using PixiEditor.Extensions.CommonApi.UserPreferences;

namespace PixiEditor.ViewModels.UserPreferences.Settings;

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

    private string primaryToolset =
        GetPreference(PreferencesConstants.PrimaryToolset, PreferencesConstants.PrimaryToolsetDefault);

    public string PrimaryToolset
    {
        get => primaryToolset;
        set => RaiseAndUpdatePreference(ref primaryToolset, value);
    }
}
