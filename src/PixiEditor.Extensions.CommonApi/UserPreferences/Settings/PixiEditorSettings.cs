namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

public static class PixiEditorSettings
{
    public static class Palettes
    {
        public static LocalSetting<IEnumerable<string>> FavouritePalettes { get; } = new(nameof(FavouritePalettes));
    }

    public static class Update
    {
        public static SyncedSetting<bool> CheckUpdatesOnStartup { get; } = new(nameof(CheckUpdatesOnStartup), true);

        public static SyncedSetting<string> UpdateChannel { get; } = new(nameof(UpdateChannel));
    }

    public static class Debug
    {
        public static SyncedSetting<bool> IsDebugModeEnabled { get; } = new(nameof(IsDebugModeEnabled));

        public static LocalSetting<string> PoEditorApiKey { get; } = new("POEditor_API_Key");
    }
    
    public static class Tools
    {
        public static SyncedSetting<bool> EnableSharedToolbar { get; } = new(nameof(EnableSharedToolbar));

        // TODO: Use RightClickMode
        public static SyncedSetting<object> RightClickMode { get; } = new(nameof(RightClickMode));
        
        public static SyncedSetting<bool> IsPenModeEnabled { get; } = new(nameof(IsPenModeEnabled));
    }

    public static class File
    {
        public static SyncedSetting<int> DefaultNewFileWidth { get; } = new(nameof(DefaultNewFileWidth), 64);

        public static SyncedSetting<int> DefaultNewFileHeight { get; } = new(nameof(DefaultNewFileHeight), 64);
        
        public static LocalSetting<IEnumerable<string>> RecentlyOpened { get; } = new(nameof(RecentlyOpened));
    
        public static SyncedSetting<int> MaxOpenedRecently { get; } = new(nameof(MaxOpenedRecently), 8);
    }
    
    public static class StartupWindow
    {
        public static SyncedSetting<bool> ShowStartupWindow { get; } = new(nameof(ShowStartupWindow), true);

        public static SyncedSetting<bool> DisableNewsPanel { get; } = new(nameof(DisableNewsPanel));
    
        public static SyncedSetting<bool> NewsPanelCollapsed { get; } = new(nameof(NewsPanelCollapsed));

        public static SyncedSetting<IEnumerable<int>> LastCheckedNewsIds { get; } = new(nameof(LastCheckedNewsIds));
    }
    
    
    public static class Discord
    {
        public static SyncedSetting<bool> EnableRichPresence { get; } = new(nameof(EnableRichPresence));

        public static SyncedSetting<bool> ShowDocumentName { get; } = new(nameof(ShowDocumentName));

        public static SyncedSetting<bool> ShowDocumentSize { get; } = new(nameof(ShowDocumentSize), true);

        public static SyncedSetting<bool> ShowLayerCount { get; } = new(nameof(ShowLayerCount), true);
    }
}
