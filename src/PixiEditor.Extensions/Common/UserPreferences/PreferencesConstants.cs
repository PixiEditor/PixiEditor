namespace PixiEditor.Extensions.Common.UserPreferences;

public static class PreferencesConstants
{
    public const string FavouritePalettes = nameof(FavouritePalettes);
    
    public const string RecentlyOpened = nameof(RecentlyOpened);

    public const string MaxOpenedRecently = nameof(MaxOpenedRecently);
    public const int MaxOpenedRecentlyDefault = 8;
    
    public const string DisableNewsPanel = nameof(DisableNewsPanel);
    
    public const string LastCheckedNewsIds = nameof(LastCheckedNewsIds);
    
    public const string NewsPanelCollapsed = nameof(NewsPanelCollapsed);
    
    public const string AutosavePeriodMinutes = nameof(AutosavePeriodMinutes);
    public const double AutosavePeriodDefault = 3;

    public const string UnsavedNextSessionFiles = nameof(UnsavedNextSessionFiles);

    public const string AutosaveToDocumentPath = nameof(AutosaveToDocumentPath);
    public const bool AutosaveToDocumentPathDefault = false;
    
    public const string SaveSessionStateEnabled = nameof(SaveSessionStateEnabled);
    public const bool SaveSessionStateDefault = true;
}
