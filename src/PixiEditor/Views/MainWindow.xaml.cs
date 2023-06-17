using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using PixiEditor.Models.UserPreferences;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools;

namespace PixiEditor.Views;

internal partial class MainWindow : Window
{
    private static WriteableBitmap pixiEditorLogo;

    private readonly IPreferences preferences;

    private readonly IServiceProvider services;

    public static MainWindow Current { get; private set; }

    public new ViewModelMain DataContext { get => (ViewModelMain)base.DataContext; set => base.DataContext = value; }

    public event Action OnDataContextInitialized;

    public MainWindow()
    {
        Current = this;

        services = new ServiceCollection()
            .AddPixiEditor()
            .BuildServiceProvider();

        SkiaDrawingBackend skiaDrawingBackend = new SkiaDrawingBackend();
        DrawingBackendApi.SetupBackend(skiaDrawingBackend);

        preferences = services.GetRequiredService<IPreferences>();
        DataContext = services.GetRequiredService<ViewModelMain>();
        DataContext.Setup(services);

        InitializeComponent();

        OnDataContextInitialized?.Invoke();
        pixiEditorLogo = BitmapFactory.FromResource(@"/Images/PixiEditorLogo.png");

        UpdateWindowChromeBorderThickness();
        StateChanged += MainWindow_StateChanged;

        DataContext.CloseAction = Close;
        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        ContentRendered += MainWindow_ContentRendered;

        preferences.AddCallback<bool>("ImagePreviewInTaskbar", x =>
        {
            UpdateTaskbarIcon(x ? DataContext?.DocumentManagerSubViewModel.ActiveDocument : null);
        });

        DataContext.DocumentManagerSubViewModel.ActiveDocumentChanged += DocumentChanged;
    }

    private void MainWindow_ContentRendered(object sender, EventArgs e)
    {
        GlobalMouseHook.Instance.Initilize(this);
    }

    public static MainWindow CreateWithDocuments(IEnumerable<(string? originalPath, byte[] dotPixiBytes)> documents)
    {
        MainWindow window = new();
        FileViewModel fileVM = window.services.GetRequiredService<FileViewModel>();

        foreach (var (path, bytes) in documents)
        {
            fileVM.OpenRecoveredDotPixi(path, bytes);
        }

        return window;
    }

    /// <summary>Brings main window to foreground.</summary>
    public void BringToForeground()
    {
        if (WindowState == WindowState.Minimized || this.Visibility == Visibility.Hidden)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        Activate();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        DataContext.CloseWindow(e);
        DataContext.DiscordViewModel.Dispose();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ((HwndSource)PresentationSource.FromVisual(this)).AddHook(Helpers.WindowSizeHelper.SetMaxSizeHook);
    }

    private void DocumentChanged(object sender, Models.Events.DocumentChangedEventArgs e)
    {
        if (preferences.GetPreference("ImagePreviewInTaskbar", false))
        {
            UpdateTaskbarIcon(e.NewDocument);
        }
    }

    private void UpdateTaskbarIcon(DocumentViewModel document)
    {
        if (document?.PreviewBitmap is null)
        {
            Icon = pixiEditorLogo;
            return;
        }

        WriteableBitmap previewCopy = document.PreviewBitmap.Clone()
            .Resize(512, 512, WriteableBitmapExtensions.Interpolation.NearestNeighbor);

        previewCopy.Blit(new Rect(256, 256, 256, 256), pixiEditorLogo, new Rect(0, 0, 512, 512));

        Icon = previewCopy;
    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.MaximizeWindow(this);
    }

    private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.RestoreWindow(this);
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void UpdateWindowChromeBorderThickness()
    {
        if (WindowState == WindowState.Maximized)
        {
            windowsChrome.ResizeBorderThickness = new Thickness(0, 0, 0, 0);
        }
        else
        {
            windowsChrome.ResizeBorderThickness = new Thickness(5, 5, 5, 5);
        }
    }

    private void MainWindow_StateChanged(object sender, EventArgs e)
    {
        UpdateWindowChromeBorderThickness();

        if (WindowState == WindowState.Maximized)
        {
            RestoreButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            RestoreButton.Visibility = Visibility.Collapsed;
            MaximizeButton.Visibility = Visibility.Visible;
        }
    }

    private void MainWindow_Initialized(object sender, EventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => Helpers.CrashHelper.SaveCrashInfo((Exception)e.ExceptionObject);
    }

    private void MainWindow_Drop(object sender, DragEventArgs e)
    {
        DataContext.ActionDisplays[nameof(MainWindow_Drop)] = null;
        
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            if (!ColorHelper.ParseAnyFormat(e.Data, out var color))
            {
                return;
            }

            e.Effects = DragDropEffects.Copy;
            DataContext.ColorsSubViewModel.PrimaryColor = color.Value;
            return;
        }

        Activate();
        string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
        
        if (files is { Length: > 0 } && Importer.IsSupportedFile(files[0]))
        {
            DataContext.FileSubViewModel.OpenFromPath(files[0]);
        }
    }

    private void MainWindow_DragEnter(object sender, DragEventArgs e)
    {
        if (!ClipboardController.IsImage((DataObject)e.Data))
        {
            if (ColorHelper.ParseAnyFormat(e.Data, out _))
            {
                DataContext.ActionDisplays[nameof(MainWindow_Drop)] = "PASTE_AS_PRIMARY_COLOR";
                e.Effects = DragDropEffects.Copy;
                return;
            }
            
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        DataContext.ActionDisplays[nameof(MainWindow_Drop)] = "IMPORT_AS_NEW_FILE";
    }

    private void MainWindow_DragLeave(object sender, DragEventArgs e)
    {
        DataContext.ActionDisplays[nameof(MainWindow_Drop)] = null;
    }

    private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.System) // Disables alt menu item navigation, I hope it won't break anything else.
        {
            e.Handled = true;
        }
    }

    private void Viewport_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (DataContext.ToolsSubViewModel.RightClickMode != RightClickMode.ContextMenu &&
            !DataContext.ToolsSubViewModel.ActiveTool.AlwaysShowContextMenu)
        {
            e.Handled = true;
        }
    }
}
