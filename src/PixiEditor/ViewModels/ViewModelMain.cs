using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.Bridge;
using Microsoft.Extensions.DependencyInjection;
using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Collections;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Config;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.DocumentModels.Autosave;
using PixiEditor.Models.ExtensionServices;
using PixiEditor.Models.Files;
using PixiEditor.Models.Handlers;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.ViewModels.Document.Nodes.Brushes;
using PixiEditor.ViewModels.Menu;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.SubViewModels.AdditionalContent;
using PixiEditor.ViewModels.Tools;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.ViewModels;

internal partial class ViewModelMain : ViewModelBase, ICommandsHandler
{
    public static ViewModelMain Current { get; set; }
    public IServiceProvider Services { get; private set; }

    public event Action OnClose;
    public event Action OnEarlyStartupEvent;
    public event Action OnStartupEvent;
    public FileViewModel FileSubViewModel { get; set; }
    public UpdateViewModel UpdateSubViewModel { get; set; }
    public IToolsHandler ToolsSubViewModel { get; set; }
    public IoViewModel IoSubViewModel { get; set; }
    public LayersViewModel LayersSubViewModel { get; set; }
    public ClipboardViewModel ClipboardSubViewModel { get; set; }
    public UndoViewModel UndoSubViewModel { get; set; }
    public SelectionViewModel SelectionSubViewModel { get; set; }
    public ViewOptionsViewModel ViewportSubViewModel { get; set; }
    public ColorsViewModel ColorsSubViewModel { get; set; }
    public MiscViewModel MiscSubViewModel { get; set; }
    public DiscordViewModel DiscordViewModel { get; set; }
    public DebugViewModel DebugSubViewModel { get; set; }
    public DocumentManagerViewModel DocumentManagerSubViewModel { get; set; }
    public CommandController CommandController { get; set; }
    public ShortcutController ShortcutController { get; set; }
    public StylusViewModel StylusSubViewModel { get; set; }
    public WindowViewModel WindowSubViewModel { get; set; }
    public SearchViewModel SearchSubViewModel { get; set; }
    public RegistryViewModel RegistrySubViewModel { get; set; }
    public AdditionalContentViewModel AdditionalContentSubViewModel { get; set; }
    public ExtensionsViewModel ExtensionsSubViewModel { get; set; }
    public LayoutViewModel LayoutSubViewModel { get; set; }
    public MenuBarViewModel MenuBarViewModel { get; set; }
    public AnimationsViewModel AnimationsSubViewModel { get; set; }
    public NodeGraphManagerViewModel NodeGraphManager { get; set; }
    public AutosaveViewModel AutosaveViewModel { get; set; }
    public UserViewModel UserViewModel { get; set; }
    public BrushesViewModel BrushesSubViewModel { get; set; }

    public IPreferences Preferences { get; set; }
    public ILocalizationProvider LocalizationProvider { get; set; }

    public ConfigManager Config { get; set; }

    public LocalizedString ActiveActionDisplay
    {
        get
        {
            if (ActionDisplays.HasActive())
            {
                return ActionDisplays.GetActive();
            }

            var documentDisplay = DocumentManagerSubViewModel.ActiveDocument?.ActionDisplays;
            if (documentDisplay != null && documentDisplay.HasActive())
            {
                return documentDisplay.GetActive();
            }

            return ToolsSubViewModel.ActiveTool?.ActionDisplay ?? default;
        }
    }

    public ActionDisplayList ActionDisplays { get; }
    public bool UserWantsToClose { get; private set; }
    public Guid CurrentSessionId { get; } = Guid.NewGuid();
    public DateTime LaunchDateTime { get; } = DateTime.Now;

    public bool IsUserReady { get; set; } = false;
    public event Action OnUserReady;

    public event Action<DocumentViewModel> BeforeDocumentClosed;
    public event Action<LazyDocumentViewModel> LazyDocumentClosed;
    
    public event Action<MainWindow> AttachedToWindow;

    public MainWindow? AttachedWindow { get; private set; }

    public Func<EditorData> GetEditorData { get; private set; }

    public ViewModelMain()
    {
        Current = this;
        ActionDisplays = new ActionDisplayList(() => OnPropertyChanged(nameof(ActiveActionDisplay)));
    }

    public void Setup(IServiceProvider services)
    {
        Services = services;

        Config = new ConfigManager();

        Preferences = services.GetRequiredService<IPreferences>();
        Preferences.Init();

        SupportedFilesHelper.InitFileTypes(services.GetServices<IoFileType>());

        CommandController = services.GetService<CommandController>();

        LocalizationProvider = services.GetRequiredService<ILocalizationProvider>();
        string code = Preferences.GetPreference<string>("LanguageCode", null);
        LocalizationProvider.LoadData(code);

        WindowSubViewModel = services.GetService<WindowViewModel>();
        LayoutSubViewModel = services.GetService<LayoutViewModel>();

        DocumentManagerSubViewModel = services.GetRequiredService<DocumentManagerViewModel>();
        SelectionSubViewModel = services.GetService<SelectionViewModel>();

        FileSubViewModel = services.GetService<FileViewModel>();
        ToolsSubViewModel = services.GetService<IToolsHandler>();
        ToolsSubViewModel.SelectedToolChanged += ToolsSubViewModel_SelectedToolChanged;

        IoSubViewModel = services.GetService<IoViewModel>();
        LayersSubViewModel = services.GetService<LayersViewModel>();
        ClipboardSubViewModel = services.GetService<ClipboardViewModel>();
        UndoSubViewModel = services.GetService<UndoViewModel>();
        ViewportSubViewModel = services.GetService<ViewOptionsViewModel>();
        ColorsSubViewModel = services.GetService<ColorsViewModel>();
        ColorsSubViewModel?.SetupPaletteProviders(services);

        BrushesSubViewModel = services.GetService<BrushesViewModel>();

        ToolsConfig toolSetConfig = Config.GetConfig<ToolsConfig>("ToolSetsConfig");

        ToolsSubViewModel?.SetupTools(services, toolSetConfig);

        DiscordViewModel = services.GetService<DiscordViewModel>();
        UpdateSubViewModel = services.GetService<UpdateViewModel>();
        DebugSubViewModel = services.GetService<DebugViewModel>();

        StylusSubViewModel = services.GetService<StylusViewModel>();
        RegistrySubViewModel = services.GetService<RegistryViewModel>();

        ExtensionsSubViewModel = services.GetService<ExtensionsViewModel>();

        UserViewModel = services.GetRequiredService<UserViewModel>();
        AdditionalContentSubViewModel = services.GetService<AdditionalContentViewModel>();
        MenuBarViewModel = new MenuBarViewModel(AdditionalContentSubViewModel, UpdateSubViewModel, UserViewModel);

        CommandController.Init(services);
        LayoutSubViewModel.LayoutManager.InitLayout(this);

        MiscSubViewModel = services.GetService<MiscViewModel>();

        ShortcutController = new ShortcutController();

        ToolsSubViewModel?.SetupToolsTooltipShortcuts();

        SearchSubViewModel = services.GetService<SearchViewModel>();

        AnimationsSubViewModel = services.GetService<AnimationsViewModel>();

        NodeGraphManager = services.GetService<NodeGraphManagerViewModel>();

        AutosaveViewModel = services.GetService<AutosaveViewModel>();

        ExtensionsSubViewModel.Init();  // Must be last

        GetEditorData = ConstructEditorData;

        DocumentManagerSubViewModel.ActiveDocumentChanged += OnActiveDocumentChanged;
        BeforeDocumentClosed += OnBeforeDocumentClosed;
        LazyDocumentClosed += OnLazyDocumentClosed;
    }

    public void OnStartup()
    {
        OnEarlyStartupEvent?.Invoke();
        OnStartupEvent?.Invoke();
        MenuBarViewModel.Init(Services, CommandController);
    }

    public bool DocumentIsNotNull(object property)
    {
        return DocumentManagerSubViewModel.ActiveDocument is not null;
    }

    public bool DocumentIsNotNull((Color oldColor, Color newColor) obj)
    {
        return DocumentIsNotNull(null);
    }

    [RelayCommand]
    public async Task CloseWindow()
    {
        ResetNextSessionFiles();
        UserWantsToClose = await DisposeAllDocumentsWithSaveConfirmation();

        if (UserWantsToClose)
        {
            var analytics = Services.GetService<AnalyticsPeriodicReporter>();
            if (analytics != null)
            {
                await analytics.StopAsync();
            }

            OnClose?.Invoke();
        }
    }

    public void ResetNextSessionFiles()
    {
        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.NextSessionFiles, Array.Empty<SessionFile>());
    }

    private void ToolsSubViewModel_SelectedToolChanged(object sender, SelectedToolEventArgs e)
    {
        if (e.OldTool != null)
            ((ToolViewModel)e.OldTool).PropertyChanged -= SelectedTool_PropertyChanged;
        if (e.NewTool != null)
            ((ToolViewModel)e.NewTool).PropertyChanged += SelectedTool_PropertyChanged;

        NotifyToolActionDisplayChanged();
    }

    private void SelectedTool_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ToolViewModel.ActionDisplay))
        {
            NotifyToolActionDisplayChanged();
        }
    }

    public void NotifyToolActionDisplayChanged()
    {
        if (!ActionDisplays.Any()) OnPropertyChanged(nameof(ActiveActionDisplay));
    }

    /// <summary>
    /// Closes documents with unsaved changes confirmation dialog.
    /// </summary>
    /// <returns>If documents was removed successfully.</returns>
    private async Task<bool> DisposeAllDocumentsWithSaveConfirmation()
    {
        int docCount = DocumentManagerSubViewModel.Documents.Count;
        for (int i = 0; i < docCount; i++)
        {
            WindowSubViewModel.MakeDocumentViewportActive(DocumentManagerSubViewModel.Documents.First());
            bool canceled = !await DisposeActiveDocumentWithSaveConfirmation();
            if (canceled)
            {
                return false;
            }
        }

        int lazyDocCount = DocumentManagerSubViewModel.LazyDocuments.Count;
        for (int i = 0; i < lazyDocCount; i++)
        {
            var lazyDoc = DocumentManagerSubViewModel.LazyDocuments.First();
            CloseLazyDocument(lazyDoc);
            WindowSubViewModel.CloseViewportForLazyDocument(lazyDoc);
        }

        return true;
    }

    private void OnBeforeDocumentClosed(DocumentViewModel document)
    {
        if (!AutosaveViewModel.SaveSessionStateEnabled || DebugSubViewModel.ModifiedEditorData)
            return;

        document.AutosaveViewModel.AutosaveOnClose();

        List<SessionFile> sessionFiles = IPreferences.Current
            .GetLocalPreference<SessionFile[]>(PreferencesConstants.NextSessionFiles)?.ToList() ?? new();
        sessionFiles.RemoveAll(x =>
            (x.OriginalFilePath != null && x.OriginalFilePath == document.FullFilePath) ||
            (x.AutosaveFilePath != null && x.AutosaveFilePath == document.AutosaveViewModel.LastAutosavedPath));
        sessionFiles.Add(new SessionFile(document.FullFilePath, document.AutosaveViewModel.LastAutosavedPath));

        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.NextSessionFiles, sessionFiles.ToArray());
    }

    private void OnLazyDocumentClosed(LazyDocumentViewModel document)
    {
        List<SessionFile> sessionFiles = IPreferences.Current
            .GetLocalPreference<SessionFile[]>(PreferencesConstants.NextSessionFiles)?.ToList() ?? new();
        sessionFiles.RemoveAll(x =>
            (x.OriginalFilePath != null && x.OriginalFilePath == document.OriginalPath) ||
            (x.AutosaveFilePath != null && x.AutosaveFilePath == document.AutosavePath));

        sessionFiles.Add(new SessionFile(document.OriginalPath, document.AutosavePath));

        IPreferences.Current.UpdateLocalPreference(PreferencesConstants.NextSessionFiles, sessionFiles.ToArray());
    }

    internal void CloseLazyDocument(LazyDocumentViewModel document)
    {
        DocumentManagerSubViewModel.LazyDocuments.Remove(document);
        LazyDocumentClosed?.Invoke(document);
    }

    /// <summary>
    /// Disposes the active document after showing the unsaved changes confirmation dialog.
    /// </summary>
    /// <returns>If the document was closed successfully.</returns>
    public async Task<bool> DisposeActiveDocumentWithSaveConfirmation()
    {
        if (DocumentManagerSubViewModel.ActiveDocument is null)
            return false;
        return await DisposeDocumentWithSaveConfirmation(DocumentManagerSubViewModel.ActiveDocument);
    }

    public async Task<bool> DisposeDocumentWithSaveConfirmation(DocumentViewModel document)
    {
        const string ConfirmationDialogTitle = "UNSAVED_CHANGES";
        const string ConfirmationDialogMessage = "DOCUMENT_MODIFIED_SAVE";

        ConfirmationType result = ConfirmationType.No;
        bool saved = false;
        if (!document.AllChangesSaved)
        {
            result = await ConfirmationDialog.Show(ConfirmationDialogMessage, ConfirmationDialogTitle);
            if (result == ConfirmationType.Yes)
            {
                if (!await FileSubViewModel.SaveDocument(document, false))
                    return false;

                saved = true;
            }
        }

        if (result != ConfirmationType.Canceled)
        {
            using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
            BeforeDocumentClosed?.Invoke(document);
            if (!DocumentManagerSubViewModel.Documents.Remove(document))
            {
#if DEBUG
                throw new InvalidOperationException(
                    "Trying to close a document that's not in the documents collection. Likely, the document wasn't added there after creation by mistake.");
#endif
            }

            if (DocumentManagerSubViewModel.ActiveDocument == document)
            {
                if (DocumentManagerSubViewModel.Documents.Count > 0)
                    WindowSubViewModel.MakeDocumentViewportActive(DocumentManagerSubViewModel.Documents.Last());
                else
                    WindowSubViewModel.MakeDocumentViewportActive((DocumentViewModel)null);
            }

            WindowSubViewModel.CloseViewportsForDocument(document);
            document.Dispose();
            document.AutosaveViewModel.OnDocumentClosed();
            DocumentManagerSubViewModel.RemoveDocumentReferences(document.Id, document.NodeGraph.AllNodes.Where(x => x is NestedDocumentNodeViewModel).Select(x => x.Id));

            return true;
        }

        return false;
    }



    public void OnShutdown(ShutdownRequestedEventArgs shutdownRequestedEventArgs, Action shutdown)
    {
        shutdownRequestedEventArgs.Cancel = true;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            ResetNextSessionFiles();
            UserWantsToClose = await DisposeAllDocumentsWithSaveConfirmation();

            if (UserWantsToClose)
            {
                var analytics = Services.GetService<AnalyticsPeriodicReporter>();
                if (analytics != null)
                {
                    await analytics.StopAsync();
                }

                OnClose?.Invoke();
                shutdown();
            }
        });
    }

    public EditorData ConstructEditorData()
    {
        return new EditorData(ColorsSubViewModel.PrimaryColor, ColorsSubViewModel.SecondaryColor);
    }

    private void OnActiveDocumentChanged(object sender, DocumentChangedEventArgs e)
    {
        NotifyToolActionDisplayChanged();
        if (e.OldDocument is not null)
            e.OldDocument.SizeChanged -= ActiveDocument_DocumentSizeChanged;
        if (e.NewDocument is not null)
            e.NewDocument.SizeChanged += ActiveDocument_DocumentSizeChanged;
    }

    private void ActiveDocument_DocumentSizeChanged(object sender, DocumentSizeChangedEventArgs e)
    {
        foreach (var viewport in WindowSubViewModel.Viewports.Where(viewport => viewport.Document == e.Document))
        {
            viewport.CenterViewportTrigger.Execute(this, viewport.GetRenderOutputSize());
        }
    }

    public void AttachToWindow(MainWindow mainWindow)
    {
        AttachedWindow = mainWindow;
        AttachedToWindow?.Invoke(mainWindow);
    }

    internal void InvokeUserReadyEvent()
    {
        IsUserReady = true;
        OnUserReady?.Invoke();
    }
}
