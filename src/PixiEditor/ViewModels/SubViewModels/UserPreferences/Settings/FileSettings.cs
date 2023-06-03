using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Models;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences.Settings;

internal class FileSettings : SettingsGroup
{
    private bool showStartupWindow = GetPreference(nameof(ShowStartupWindow), true);

    public bool ShowStartupWindow
    {
        get => showStartupWindow;
        set => RaiseAndUpdatePreference(ref showStartupWindow, value);
    }

    private int defaultNewFileWidth = GetPreference("DefaultNewFileWidth", Constants.DefaultCanvasSize);

    public int DefaultNewFileWidth
    {
        get => defaultNewFileWidth;
        set
        {
            defaultNewFileWidth = value;
            string name = nameof(DefaultNewFileWidth);
            RaiseAndUpdatePreference(name, value);
        }
    }

    private int defaultNewFileHeight = GetPreference("DefaultNewFileHeight", Constants.DefaultCanvasSize);

    public int DefaultNewFileHeight
    {
        get => defaultNewFileHeight;
        set
        {
            defaultNewFileHeight = value;
            string name = nameof(DefaultNewFileHeight);
            RaiseAndUpdatePreference(name, value);
        }
    }

    private int maxOpenedRecently = GetPreference(PreferencesConstants.MaxOpenedRecently, PreferencesConstants.MaxOpenedRecentlyDefault);

    public int MaxOpenedRecently
    {
        get => maxOpenedRecently;
        set => RaiseAndUpdatePreference(ref maxOpenedRecently, value);
    }
}
