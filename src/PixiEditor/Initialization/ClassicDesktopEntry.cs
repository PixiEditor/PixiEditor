using System.Diagnostics;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Behaviours;
using PixiEditor.IdentityProvider;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.ExceptionHandling;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.Platform;
using PixiEditor.UI.Common.Controls;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.Views;
using PixiEditor.Views.Auth;
using PixiEditor.Views.Dialogs;
using ViewModels_ViewModelMain = PixiEditor.ViewModels.ViewModelMain;

namespace PixiEditor.Initialization;

internal class ClassicDesktopEntry
{
    public ServiceProvider? Services { get; private set; }
    private IClassicDesktopStyleApplicationLifetime desktop;
    private bool restartQueued = false;

    private LoginPopup? loginPopup = null!;

    public static ClassicDesktopEntry? Active { get; private set; }

    public ClassicDesktopEntry(IClassicDesktopStyleApplicationLifetime desktop)
    {
        this.desktop = desktop;
        IActivatableLifetime? activable =
            (IActivatableLifetime?)App.Current.TryGetFeature(typeof(IActivatableLifetime));
        if (activable != null)
        {
            activable.Activated += ActivableOnActivated;
        }

        Active = this;

        desktop.Startup += Start;
        desktop.ShutdownRequested += ShutdownRequested;
    }

    private void ActivableOnActivated(object? sender, ActivatedEventArgs e)
    {
        // TODO: Handle activation more generically. This only is handled by macos btw.
        if (desktop.MainWindow is not MainWindow mainWindow) return;
        if (e.Kind == ActivationKind.File && e is FileActivatedEventArgs fileActivatedEventArgs)
        {
            foreach (var storageItem in fileActivatedEventArgs.Files)
            {
                string? file = storageItem.TryGetLocalPath();
                if (file != null && File.Exists(file))
                {
                    mainWindow.DataContext.FileSubViewModel.OpenFromPath(file);
                }
            }
        }
        else if (e.Kind == ActivationKind.OpenUri && e is ProtocolActivatedEventArgs openUriEventArgs)
        {
            var uri = openUriEventArgs.Uri;
            if (uri.Scheme == "lospec-palette")
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                    await mainWindow.DataContext.ColorsSubViewModel.ImportLospecPalette(uri.AbsoluteUri));
            }
        }
    }

    private void Start(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        StartupArgs.Args = e.Args.ToList();
        string arguments = string.Join(' ', e.Args);

        InitOperatingSystem();

        bool safeMode = arguments.Contains("--safeMode", StringComparison.OrdinalIgnoreCase);

        if (ParseArgument(@"--crash (""?)([\w:\/\ -_.]+)\1", arguments, out Group[] groups))
        {
            try
            {
                CrashReport report = CrashReport.Parse(groups[2].Value);
                desktop.MainWindow = new CrashReportDialog(report);
                desktop.MainWindow.Show();
            }
            catch (Exception exception)
            {
                try
                {
                    CrashHelper.SendExceptionInfo(exception, true);
                }
                finally
                {
                    // TODO: find an avalonia replacement for messagebox 
                    //MessageBox.Show("Fatal error", $"Fatal error while trying to open crash report in App.OnStartup()\n{exception}");
                }
            }

            return;
        }

#if !STEAM && !DEBUG
        if (!HandleNewInstance(Dispatcher.UIThread))
        {
            return;
        }
#endif

        var extensionLoader = InitApp(safeMode);
        ViewModels_ViewModelMain viewModel = Services.GetRequiredService<ViewModels_ViewModelMain>();
        viewModel.Setup(Services);

#if FOUNDERS_PACK_REQUIRED
        if (!IsFoundersPackOwner())
        {
            IPlatform.Current.IdentityProvider.OwnedProductsUpdated += IdentityProviderOnOwnedProductsUpdated;
            loginPopup = new LoginPopup();
            loginPopup.SystemDecorations = SystemDecorations.BorderOnly;
            loginPopup.CanMinimize = false;
            loginPopup.DataContext = viewModel.UserViewModel;
            loginPopup.ShowStandalone();
            loginPopup.Closed += (_, _) =>
            {
                if (IsFoundersPackOwner())
                {
                    desktop.MainWindow = new MainWindow(extensionLoader);
                    desktop.MainWindow.Show();
                }
                else
                {
                    desktop.Shutdown();
                }
            };
        }
        else
        {
            
            desktop.MainWindow = new MainWindow(extensionLoader);
            desktop.MainWindow.Show();
        }
#else
        desktop.MainWindow = new MainWindow(extensionLoader);
        desktop.MainWindow.Show();
#endif
    }

    private void IdentityProviderOnOwnedProductsUpdated(List<ProductData> obj)
    {
        if (IsFoundersPackOwner())
        {
            IPlatform.Current.IdentityProvider.OwnedProductsUpdated -= IdentityProviderOnOwnedProductsUpdated;
            loginPopup?.Close();
            loginPopup = null!;
        }
    }

    private static bool IsFoundersPackOwner()
    {
        return IPlatform.Current.IdentityProvider.IsLoggedIn &&
               IPlatform.Current.AdditionalContentProvider.IsContentOwned("PixiEditor.FoundersPack");
    }

    private void InitPlatform()
    {
        var platform = GetActivePlatform();
        IPlatform.RegisterPlatform(platform);
        platform.PerformHandshake();
    }

    public ExtensionLoader InitApp(bool safeMode)
    {
        LoadingWindow.ShowInNewThread();

        InitPlatform();

        NumberInput.AttachGlobalBehaviors += AttachGlobalShortcutBehavior;

        if (!Directory.Exists(Paths.LocalExtensionPackagesPath))
        {
            Directory.CreateDirectory(Paths.LocalExtensionPackagesPath);
        }

        ExtensionLoader extensionLoader = new ExtensionLoader(
            [Paths.InstallDirExtensionPackagesPath, Paths.LocalExtensionPackagesPath], Paths.UserExtensionsPath);
        if (!safeMode)
        {
            extensionLoader.LoadExtensions();
        }

        Services = new ServiceCollection()
            .AddPlatform()
            .AddPixiEditor(extensionLoader)
            .AddExtensionServices(extensionLoader)
            .BuildServiceProvider();

        extensionLoader.Services = new ExtensionServices(Services);

        return extensionLoader;
    }

    private IPlatform GetActivePlatform()
    {
#if STEAM || DEV_STEAM
        return new PixiEditor.Platform.Steam.SteamPlatform();
#elif MSIX || MSIX_DEBUG
        return new PixiEditor.Platform.MSStore.MicrosoftStorePlatform();
#else
        return new PixiEditor.Platform.Standalone.StandalonePlatform(Paths.LocalExtensionPackagesPath, GetApiUrl());
#endif
    }

    private void InitOperatingSystem()
    {
        IOperatingSystem.RegisterOS(GetActiveOperatingSystem());
    }

    private IOperatingSystem GetActiveOperatingSystem()
    {
#if WINDOWS
        return new PixiEditor.Windows.WindowsOperatingSystem();
#elif LINUX
        return new PixiEditor.Linux.LinuxOperatingSystem();
#elif MACOS
        return new PixiEditor.MacOs.MacOperatingSystem();
#else
        throw new PlatformNotSupportedException("This platform is not supported");
#endif
    }

    private bool HandleNewInstance(Dispatcher? dispatcher)
    {
        return IOperatingSystem.Current.HandleNewInstance(dispatcher, OpenInExisting, desktop);
    }

    private void OpenInExisting(string passedArgs, bool isInline)
    {
        if (desktop.MainWindow is MainWindow mainWindow)
        {
            mainWindow.BringIntoView();
            List<string> args = new List<string>();
            if (isInline)
            {
                args = CommandLineHelpers.SplitCommandLine(passedArgs)
                    .ToList();
            }
            else if (File.Exists(passedArgs))
            {
                args = CommandLineHelpers.SplitCommandLine(File.ReadAllText(passedArgs))
                    .ToList();
                File.Delete(passedArgs);
            }

            StartupArgs.Args = args;
            StartupArgs.Args.Add("--openedInExisting");
            ViewModels_ViewModelMain viewModel = (ViewModels_ViewModelMain)mainWindow.DataContext;
            viewModel.OnStartup();
        }
    }

    private bool ParseArgument(string pattern, string args, out Group[] groups)
    {
        Match match = Regex.Match(args, pattern, RegexOptions.IgnoreCase);
        groups = null;

        if (match.Success)
        {
            groups = match.Groups.Values.ToArray();
        }

        return match.Success;
    }

    public void Restart()
    {
        restartQueued = true;
        desktop.TryShutdown();
    }

    private void ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        // TODO: Make sure this works
        var vm = ViewModels_ViewModelMain.Current;
        if (vm is null)
            return;

        vm.OnShutdown(e, () =>
        {
            desktop.Shutdown(0);
            if (restartQueued)
            {
                var process = Process.GetCurrentProcess().MainModule.FileName;
                desktop.Exit += (_, _) =>
                {
                    Process.Start(process);
                };
            }
        });
    }

    private void AttachGlobalShortcutBehavior(BehaviorCollection collection)
    {
        if (collection is null)
            return;

        collection.Add(new GlobalShortcutFocusBehavior());
    }

    private string GetApiUrl()
    {
        string baseUrl = BuildConstants.PixiEditorApiUrl;
#if DEBUG
        if (baseUrl.Contains('{') && baseUrl.Contains('}'))
        {
            string? envUrl = Environment.GetEnvironmentVariable("PIXIEDITOR_API_URL");
            if (envUrl != null)
            {
                baseUrl = envUrl;
            }
        }
#endif

        return baseUrl;
    }
}
