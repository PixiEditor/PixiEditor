namespace PixiEditor.Extensions.CommonApi.UserPreferences;

public static class PreferencesConstants
{
    public const string FavouritePalettes = "FavouritePalettes";
    public const string RecentlyOpened = "RecentlyOpened";

    public const string MaxOpenedRecently = "MaxOpenedRecently";
    public const int MaxOpenedRecentlyDefault = 8;
    public const string DisableNewsPanel = "DisableNewsPanel";
    public const string LastCheckedNewsIds = "LastCheckedNewsIds";
    public const string NewsPanelCollapsed = "NewsPanelCollapsed";

    public const string SaveSessionStateEnabled = "SaveSessionStateEnabled";
    public const bool SaveSessionStateDefault = true;

    public const string AutosaveEnabled = "AutosaveEnabled";
    public const bool AutosaveEnabledDefault = true;

    public const string AutosaveHistory = "AutosaveHistory";

    public const string AutosavePeriodMinutes = "AutosavePeriodMinutes";
    public const double AutosavePeriodDefault = 3;

    public const string AutosaveToDocumentPath = "AutosaveToDocumentPath";
    public const bool AutosaveToDocumentPathDefault = false;

    public const string LastCrashFile = "LastCrashFile";
    public const string NextSessionFiles = "NextSessionFiles";

    public const string OpenDirectoryOnExport = "OpenDirectoryOnExport";
    public const bool OpenDirectoryOnExportDefault = true;
}
