using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Models;

namespace PixiEditor.ViewModels.UserPreferences.Settings;

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

    private bool disableNewsPanel = GetPreference(PreferencesConstants.DisableNewsPanel, false);

    public bool DisableNewsPanel
    {
        get => disableNewsPanel;
        set => RaiseAndUpdatePreference(ref disableNewsPanel, value, PreferencesConstants.DisableNewsPanel);
    }

    private bool autosaveEnabled = GetPreference(PreferencesConstants.AutosaveEnabled, PreferencesConstants.AutosaveEnabledDefault);
    public bool AutosaveEnabled
    {
        get => autosaveEnabled;
        set => RaiseAndUpdatePreference(ref autosaveEnabled, value);
    }

    private bool saveSessionEnabled = GetPreference(PreferencesConstants.SaveSessionStateEnabled, PreferencesConstants.SaveSessionStateDefault);
    public bool SaveSessionEnabled
    {
        get => saveSessionEnabled;
        set => RaiseAndUpdatePreference(ref saveSessionEnabled, value);
    }

    private double autosavePeriodMinutes = GetPreference(PreferencesConstants.AutosavePeriodMinutes, PreferencesConstants.AutosavePeriodDefault);
    public double AutosavePeriodMinutes
    {
        get => autosavePeriodMinutes;
        set => RaiseAndUpdatePreference(ref autosavePeriodMinutes, value);
    }

    private bool autosaveToDocumentPath = GetPreference(PreferencesConstants.AutosaveToDocumentPath, PreferencesConstants.AutosaveToDocumentPathDefault);
    public bool AutosaveToDocumentPath
    {
        get => autosaveToDocumentPath;
        set => RaiseAndUpdatePreference(ref autosaveToDocumentPath, value);
    }
}
