namespace PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;

public static class PixiEditorSettings
{
    private const string PixiEditor = "PixiEditor";
    
    public static class Palettes
    {
        public static LocalSetting<IEnumerable<string>> FavouritePalettes { get; } = LocalSetting.NonOwned<IEnumerable<string>>(PixiEditor);
    }

    public static class Update
    {
        public static SyncedSetting<bool> CheckUpdatesOnStartup { get; } = SyncedSetting.NonOwned(PixiEditor, true);

        public static SyncedSetting<string> UpdateChannel { get; } = SyncedSetting.NonOwned<string>(PixiEditor);
    }

    public static class Debug
    {
        public static SyncedSetting<bool> IsDebugModeEnabled { get; } = SyncedSetting.NonOwned<bool>(PixiEditor);

        public static LocalSetting<string> PoEditorApiKey { get; } = new($"{PixiEditor}:POEditor_API_Key");
    }
    
    public static class Tools
    {
        public static SyncedSetting<bool> EnableSharedToolbar { get; } = SyncedSetting.NonOwned<bool>(PixiEditor);

        public static SyncedSetting<RightClickMode> RightClickMode { get; } = SyncedSetting.NonOwned<RightClickMode>(PixiEditor);
        
        public static SyncedSetting<bool> IsPenModeEnabled { get; } = SyncedSetting.NonOwned<bool>(PixiEditor);

        public static SyncedSetting<string> PrimaryToolset { get; } = SyncedSetting.NonOwned<string>(PixiEditor, "PAINT_TOOLSET");
    }

    public static class File
    {
        public static SyncedSetting<int> DefaultNewFileWidth { get; } = SyncedSetting.NonOwned(PixiEditor, 64);

        public static SyncedSetting<int> DefaultNewFileHeight { get; } = SyncedSetting.NonOwned(PixiEditor, 64);
        
        public static LocalSetting<IEnumerable<string>> RecentlyOpened { get; } = LocalSetting.NonOwned<IEnumerable<string>>(PixiEditor, []);
    
        public static SyncedSetting<int> MaxOpenedRecently { get; } = SyncedSetting.NonOwned(PixiEditor, 8);
    }
    
    public static class StartupWindow
    {
        public static SyncedSetting<bool> ShowStartupWindow { get; } = SyncedSetting.NonOwned(PixiEditor, true);

        public static SyncedSetting<bool> DisableNewsPanel { get; } = SyncedSetting.NonOwned<bool>(PixiEditor);

        public static SyncedSetting<bool> NewsPanelCollapsed { get; } = SyncedSetting.NonOwned<bool>(PixiEditor);

        public static SyncedSetting<IEnumerable<int>> LastCheckedNewsIds { get; } = SyncedSetting.NonOwned<IEnumerable<int>>(PixiEditor);
    }
    
    public static class Discord
    {
        public static SyncedSetting<bool> EnableRichPresence { get; } = SyncedSetting.NonOwned(PixiEditor, true);

        public static SyncedSetting<bool> ShowDocumentName { get; } = SyncedSetting.NonOwned<bool>(PixiEditor);

        public static SyncedSetting<bool> ShowDocumentSize { get; } = SyncedSetting.NonOwned(PixiEditor, true);

        public static SyncedSetting<bool> ShowLayerCount { get; } = SyncedSetting.NonOwned(PixiEditor, true);
    }

    public static class Analytics
    {
        public static SyncedSetting<bool> AnalyticsEnabled { get; } = SyncedSetting.NonOwned(PixiEditor, true);
    }
}
