namespace PixiEditor.Common.Performance;

public enum PerfEventType
{
    ViewModelMainSetup,
    MainWindowConstructor,
    DrawieInitialization,
    DrawieStartup,
    DrawieStartup_DrawingEngine,
    HelloTherePopupConstructor,
    CommandControllerInit,
    LayoutManagerInitLayout,
    BrushesViewModelConstructor,
    BrushLibraryLoadBrushes,
    PreferencesSettingsInitPaths,
    ClassicDesktopEntryStart,
    AppInitialize,
    ProgramBuildAvaloniaApp
}
