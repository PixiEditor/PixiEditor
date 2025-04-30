using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using Drawie.Backend.Core.Bridge;
using PixiDocks.Avalonia.Helpers;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Helpers;
using PixiEditor.Initialization;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.ExceptionHandling;
using PixiEditor.Models.IO;
using PixiEditor.Platform;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.Views.Main;
using PixiEditor.Views.Rendering;
using ViewModels_ViewModelMain = PixiEditor.ViewModels.ViewModelMain;

namespace PixiEditor.Views;

internal partial class MainWindow : Window
{
    private readonly IPreferences preferences;
    private readonly IPlatform platform;
    private readonly IServiceProvider services;
    private static ExtensionLoader extLoader;

    private MainTitleBar titleBar;

    public StartupPerformance StartupPerformance { get; } = new();

    public new ViewModels_ViewModelMain DataContext
    {
        get => (ViewModels_ViewModelMain)base.DataContext;
        set => base.DataContext = value;
    }

    public static MainWindow? Current
    {
        get
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return desktop.MainWindow as MainWindow;
            if (Application.Current is null)
                return null;
            throw new NotSupportedException("ApplicationLifetime is not supported");
        }
    }

    public MainWindow(ExtensionLoader extensionLoader, Guid? analyticsSessionId = null)
    {
        StartupPerformance.ReportToMainWindow();

        (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow = this;
        extLoader = extensionLoader;

        services = new ServiceCollection()
            .AddPlatform()
            .AddPixiEditor(extensionLoader)
            .AddExtensionServices(extensionLoader)
            .BuildServiceProvider();

        AsyncImageLoader.ImageLoader.AsyncImageLoader =
            new DiskCachedWebImageLoader(Path.Combine(Paths.TempFilesPath, "ImageCache"));

        preferences = services.GetRequiredService<IPreferences>();
        platform = services.GetRequiredService<IPlatform>();
        DataContext = services.GetRequiredService<ViewModels_ViewModelMain>();
        DataContext.Setup(services);
        StartupPerformance.ReportToMainViewModel();

        var analytics = services.GetService<AnalyticsPeriodicReporter>();
        analytics?.Start(analyticsSessionId);

        InitializeComponent();
    }


    public static MainWindow CreateWithRecoveredDocuments(CrashReport report, out bool showMissingFilesDialog)
    {
        if (!report.TryRecoverDocuments(out var documents, out var sessionInfo))
        {
            showMissingFilesDialog = true;
            return GetMainWindow(null);
        }

        var window = GetMainWindow(sessionInfo?.AnalyticsSessionId);
        var fileVM = window.services.GetRequiredService<FileViewModel>();

        var i = 0;

        fileVM.OpenFromReport(report, out showMissingFilesDialog);

        return window;

        MainWindow GetMainWindow(Guid? analyticsSession)
        {
            try
            {
                var app = (App)Application.Current;
                ClassicDesktopEntry entry = new(app.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime);
                return new MainWindow(entry.InitApp(false), analyticsSession);
            }
            catch (Exception e)
            {
                CrashHelper.SendExceptionInfo(e, true);
                throw;
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        titleBar = this.FindDescendantOfType<MainTitleBar>(true);
        if (System.OperatingSystem.IsLinux())
        {
            titleBar.PointerPressed += OnTitleBarPressed;
            
            PointerMoved += UpdateResizeCursor;
            AddHandler(PointerPressedEvent, Pressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        }
        

        LoadingWindow.Instance?.SafeClose();
        Activate();
        StartupPerformance.ReportToInteractivity();
        Analytics.SendStartup(StartupPerformance);
    }

    private void UpdateResizeCursor(object? sender, PointerEventArgs e)
    {
        if(WindowState != WindowState.Normal)
        {
            return;
        }
        
        Cursor = new Cursor(WindowUtility.SetResizeCursor(e, this, new Thickness(8)));
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Normal && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var direction = WindowUtility.GetResizeDirection(e.GetPosition(this), this, new Thickness(8));
            if(direction == null) return;
            
            BeginResizeDrag(direction.Value, e);
        }
    }

    private void OnTitleBarPressed(object? sender, PointerPressedEventArgs e)
    {
        bool withinTitleBar = e.GetPosition(this).Y <= titleBar.Bounds.Height;
        bool sourceIsMenuItem = e.Source is Control ctrl && ctrl.GetLogicalParent() is MenuItem;
        if (withinTitleBar && !sourceIsMenuItem && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if(e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                BeginMoveDrag(e);
                e.Handled = true;
            }
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!DataContext.UserWantsToClose)
        {
            e.Cancel = true;
            Task.Run(async () =>
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await DataContext.CloseWindowCommand.ExecuteAsync(null);
                    if (DataContext.UserWantsToClose)
                    {
                        Close();
                    }
                });
            });
        }

        base.OnClosing(e);
    }

    private void MainWindow_Initialized(object? sender, EventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            CrashHelper.SaveCrashInfo((Exception)e.ExceptionObject, DataContext.DocumentManagerSubViewModel.Documents);
        };
    }
}
