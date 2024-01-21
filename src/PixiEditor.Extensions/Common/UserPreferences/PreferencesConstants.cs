namespace PixiEditor.Extensions.Common.UserPreferences;

public static class PreferencesConstants
{
    [LocalPreferenceConstant]
    public const string FavouritePalettes = nameof(FavouritePalettes);
    
    [LocalPreferenceConstant]
    public const string RecentlyOpened = nameof(RecentlyOpened);

    [RemotePreferenceConstant]
    public const string MaxOpenedRecently = nameof(MaxOpenedRecently);
    public const int MaxOpenedRecentlyDefault = 8;
    
    [RemotePreferenceConstant]
    public const string DisableNewsPanel = nameof(DisableNewsPanel);
    
    [RemotePreferenceConstant]
    public const string LastCheckedNewsIds = nameof(LastCheckedNewsIds);
    
    [RemotePreferenceConstant]
    public const string NewsPanelCollapsed = nameof(NewsPanelCollapsed);
    
    [RemotePreferenceConstant]
    public const string AutosavePeriodMinutes = nameof(AutosavePeriodMinutes);
    public const double AutosavePeriodDefault = 3;

    [LocalPreferenceConstant]
    public const string UnsavedNextSessionFiles = nameof(UnsavedNextSessionFiles);

    [RemotePreferenceConstant]
    public const string AutosaveToDocumentPath = nameof(AutosaveToDocumentPath);
    public const bool AutosaveToDocumentPathDefault = false;
    
    [RemotePreferenceConstant]
    public const string SaveSessionStateEnabled = nameof(SaveSessionStateEnabled);
    public const bool SaveSessionStateDefault = true;

    [LocalPreferenceConstant]
    public const string LastCrashFile = nameof(LastCrashFile);
}
