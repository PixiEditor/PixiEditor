namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings;

public static class PixiEditorSettings
{
    public static class Palettes
    {
        public static LocalSetting<IEnumerable<string>> FavouritePalettes { get; } = LocalSetting.Owned<IEnumerable<string>>();
    }

    public static class Update
    {
        public static SyncedSetting<bool> CheckUpdatesOnStartup { get; } = SyncedSetting.Owned(true);

        public static SyncedSetting<string> UpdateChannel { get; } = SyncedSetting.Owned<string>();
    }

    public static class Debug
    {
        public static SyncedSetting<bool> IsDebugModeEnabled { get; } = SyncedSetting.Owned<bool>();

        public static LocalSetting<string> PoEditorApiKey { get; } = new("POEditor_API_Key");
    }
    
    public static class Tools
    {
        public static SyncedSetting<bool> EnableSharedToolbar { get; } = SyncedSetting.Owned<bool>();

        // TODO: Use RightClickMode
        public static SyncedSetting<object> RightClickMode { get; } = SyncedSetting.Owned<object>(0);
        
        public static SyncedSetting<bool> IsPenModeEnabled { get; } = SyncedSetting.Owned<bool>();
    }

    public static class File
    {
        public static SyncedSetting<int> DefaultNewFileWidth { get; } = SyncedSetting.Owned(64);

        public static SyncedSetting<int> DefaultNewFileHeight { get; } = SyncedSetting.Owned(64);
        
        public static LocalSetting<IEnumerable<string>> RecentlyOpened { get; } = LocalSetting.Owned<IEnumerable<string>>();
    
        public static SyncedSetting<int> MaxOpenedRecently { get; } = SyncedSetting.Owned(8);
    }
    
    public static class StartupWindow
    {
        public static SyncedSetting<bool> ShowStartupWindow { get; } = SyncedSetting.Owned(true);

        public static SyncedSetting<bool> DisableNewsPanel { get; } = SyncedSetting.Owned<bool>();
    
        public static SyncedSetting<bool> NewsPanelCollapsed { get; } = SyncedSetting.Owned<bool>();

        public static SyncedSetting<IEnumerable<int>> LastCheckedNewsIds { get; } = SyncedSetting.Owned<IEnumerable<int>>();
    }
    
    
    public static class Discord
    {
        public static SyncedSetting<bool> EnableRichPresence { get; } = SyncedSetting.Owned(true);

        public static SyncedSetting<bool> ShowDocumentName { get; } = SyncedSetting.Owned<bool>();

        public static SyncedSetting<bool> ShowDocumentSize { get; } = SyncedSetting.Owned(true);

        public static SyncedSetting<bool> ShowLayerCount { get; } = SyncedSetting.Owned(true);
    }
}
