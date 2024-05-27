namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

public static class PixiEditorSettings
{
    // TODO: Subdivide this into different classes
    
    public static LocalSetting<IEnumerable<string>> FavouritePalettes { get; } = new(nameof(FavouritePalettes));
    
    public static LocalSetting<IEnumerable<string>> RecentlyOpened { get; } = new(nameof(RecentlyOpened));
    public static SyncedSetting<int> MaxOpenedRecently { get; } = new(nameof(MaxOpenedRecently), 8);

    public static SyncedSetting<bool> ShowStartupWindow { get; } = new(nameof(ShowStartupWindow), true);

    public static SyncedSetting<bool> DisableNewsPanel { get; } = new(nameof(DisableNewsPanel));
    public static SyncedSetting<bool> NewsPanelCollapsed { get; } = new(nameof(NewsPanelCollapsed));

    public static SyncedSetting<int> DefaultNewFileWidth { get; } = new(nameof(DefaultNewFileWidth), 64);

    public static SyncedSetting<int> DefaultNewFileHeight { get; } = new(nameof(DefaultNewFileHeight));
    
    public static SyncedSetting<IEnumerable<int>> LastCheckedNewsIds { get; } = new(nameof(LastCheckedNewsIds));
    
    // Update

    public static SyncedSetting<bool> CheckUpdatesOnStartup { get; } = new(nameof(CheckUpdatesOnStartup), true);

    public static SyncedSetting<string> UpdateChannel { get; } = new(nameof(UpdateChannel));

    // Local

    public static LocalSetting<string> PoEditorApiKey { get; } = new("POEditor_API_Key");

    // Discord

    public static SyncedSetting<bool> EnableRichPresence { get; } = new(nameof(EnableRichPresence));

    public static SyncedSetting<bool> ShowDocumentName { get; } = new(nameof(ShowDocumentName));

    public static SyncedSetting<bool> ShowDocumentSize { get; } = new(nameof(ShowDocumentSize), true);
    
    public static SyncedSetting<bool> ShowLayerCount { get; } = new(nameof(ShowLayerCount), true);
    
    // Tools

    public static SyncedSetting<bool> EnableSharedToolbar { get; } = new(nameof(EnableSharedToolbar));
    
    // TODO: Use RightClickMode
    public static SyncedSetting<object> RightClickMode { get; } = new(nameof(RightClickMode));
    
    // Debug

    public static SyncedSetting<bool> IsDebugModeEnabled { get; } = new(nameof(IsDebugModeEnabled));
    
    // Pen

    public static SyncedSetting<bool> IsPenModeEnabled { get; } = new(nameof(IsPenModeEnabled));
}
