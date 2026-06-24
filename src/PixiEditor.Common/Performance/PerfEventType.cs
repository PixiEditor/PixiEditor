namespace PixiEditor.Common.Performance;

public enum PerfEventType
{
    ViewModelMain_Setup,
    MainWindow_Constructor,
    MainWindow_InitializeComponent,
    HelloTherePopup_Constructor,
    CommandController_Init,
    LayoutManager_InitLayout,
    BrushesViewModel_Constructor,
    BrushLibrary_LoadBrushes,
    PreferencesSettings_InitPaths,
    PreferencesSettings_JsonDeserialize,
    ClassicDesktopEntry_Start,
    App_Initialize,
    Program_BuildAvaloniaApp,
    SupportedFilesHelper_InitFileTypes,
    ToolsViewModel_SetupTools,
    DocumentViewModel_Build,
    MainTitleBar_Constructor,
    DiscordRichPresencePreview_Constructor,
    AboutPopup_Constructor,
    OnboardingDialog_Constructor
}
