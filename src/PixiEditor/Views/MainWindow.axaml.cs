using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Drawie.Backend.Core.Bridge;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Helpers;
using PixiEditor.Initialization;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.ExceptionHandling;
using PixiEditor.Models.IO;
using PixiEditor.Platform;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.Views.Rendering;
using ViewModels_ViewModelMain = PixiEditor.ViewModels.ViewModelMain;

namespace PixiEditor.Views;

internal partial class MainWindow : Window
{
    private readonly IPreferences preferences;
    private readonly IPlatform platform;
    private readonly IServiceProvider services;
    private static ExtensionLoader extLoader;

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

        AsyncImageLoader.ImageLoader.AsyncImageLoader = new DiskCachedWebImageLoader(Path.Combine(Paths.TempFilesPath, "ImageCache"));

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

        foreach (var document in documents)
        {
            try
            {
                fileVM.OpenRecoveredDotPixi(document.Path, document.GetRecoveredBytes());
                i++;
            }
            catch (Exception e)
            {
                CrashHelper.SendExceptionInfo(e);
            }
        }

        showMissingFilesDialog = documents.Count != i;

        return window;

        MainWindow GetMainWindow(Guid? analyticsSession)
        {
            try
            {
                var app = (App)Application.Current;
                ClassicDesktopEntry entry = new(app.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime);
                return new MainWindow(entry.InitApp(), analyticsSession);
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
        LoadingWindow.Instance?.SafeClose();
        Activate();
        StartupPerformance.ReportToInteractivity();
        Analytics.SendStartup(StartupPerformance);
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
