using System.Threading.Tasks;
using AsyncImageLoader.Loaders;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Initialization;
using PixiEditor.AvaloniaUI.Models.ExceptionHandling;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Skia;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Platform;
using ViewModelMain = PixiEditor.AvaloniaUI.ViewModels.ViewModelMain;

namespace PixiEditor.AvaloniaUI.Views;

internal partial class MainWindow : Window
{
    private readonly IPreferences preferences;
    private readonly IPlatform platform;
    private readonly IServiceProvider services;
    private static ExtensionLoader extLoader;

    public new ViewModelMain DataContext { get => (ViewModelMain)base.DataContext; set => base.DataContext = value; }
    
    public static MainWindow? Current {
        get 
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return desktop.MainWindow as MainWindow;
            if (Application.Current is null)
                return null;
            throw new NotSupportedException("ApplicationLifetime is not supported");
        }
    }

    public MainWindow(ExtensionLoader extensionLoader)
    {
        (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow = this;
        extLoader = extensionLoader;

        services = new ServiceCollection()
            .AddPlatform()
            .AddPixiEditor(extensionLoader)
            .AddExtensionServices(extensionLoader)
            .BuildServiceProvider();

        AsyncImageLoader.ImageLoader.AsyncImageLoader = new DiskCachedWebImageLoader();

        SkiaDrawingBackend skiaDrawingBackend = new SkiaDrawingBackend();
        DrawingBackendApi.SetupBackend(skiaDrawingBackend);

        preferences = services.GetRequiredService<IPreferences>();
        platform = services.GetRequiredService<IPlatform>();
        DataContext = services.GetRequiredService<ViewModelMain>();
        DataContext.Setup(services);

        InitializeComponent();
    }

    public static MainWindow CreateWithRecoveredDocuments(CrashReport report, out bool showMissingFilesDialog)
    {
        var window = GetMainWindow();
        var fileVM = window.services.GetRequiredService<FileViewModel>();

        if (!report.TryRecoverDocuments(out var documents))
        {
            showMissingFilesDialog = true;
            return window;
        }

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
                CrashHelper.SendExceptionInfoToWebhook(e);
            }
        }

        showMissingFilesDialog = documents.Count != i;

        return window;

        MainWindow GetMainWindow()
        {
            try
            {
                var app = (App)Application.Current;
                ClassicDesktopEntry entry = new(app.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime);
                return new MainWindow(entry.InitApp());
            }
            catch (Exception e)
            {
                CrashHelper.SendExceptionInfoToWebhook(e, true);
                throw;
            }
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        LoadingWindow.Instance?.SafeClose();
        Activate();
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
