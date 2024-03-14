namespace PixiEditor.Extensions.Common.UserPreferences;

public static class PreferencesConstants
{
    [LocalPreferenceConstant]
    public const string FavouritePalettes = nameof(FavouritePalettes);
    
    [LocalPreferenceConstant]
    public const string RecentlyOpened = nameof(RecentlyOpened);

    [SyncedPreferenceConstant]
    public const string MaxOpenedRecently = nameof(MaxOpenedRecently);
    public const int MaxOpenedRecentlyDefault = 8;
    
    [SyncedPreferenceConstant]
    public const string DisableNewsPanel = nameof(DisableNewsPanel);
    
    [SyncedPreferenceConstant]
    public const string LastCheckedNewsIds = nameof(LastCheckedNewsIds);
    
    [SyncedPreferenceConstant]
    public const string NewsPanelCollapsed = nameof(NewsPanelCollapsed);
    
    [SyncedPreferenceConstant]
    public const string AutosavePeriodMinutes = nameof(AutosavePeriodMinutes);
    public const double AutosavePeriodDefault = 3;
    
    [SyncedPreferenceConstant]
    public const string AutosaveEnabled = nameof(AutosaveEnabled);
    public const bool AutosaveEnabledDefault = true;

    [LocalPreferenceConstant]
    public const string AutosaveHistory = nameof(AutosaveHistory);

    [SyncedPreferenceConstant]
    public const string AutosaveToDocumentPath = nameof(AutosaveToDocumentPath);
    public const bool AutosaveToDocumentPathDefault = false;
    
    [SyncedPreferenceConstant]
    public const string SaveSessionStateEnabled = nameof(SaveSessionStateEnabled);
    public const bool SaveSessionStateDefault = true;

    [LocalPreferenceConstant]
    public const string LastCrashFile = nameof(LastCrashFile);
}
