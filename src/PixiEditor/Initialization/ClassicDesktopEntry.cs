using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Runtime;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.ExceptionHandling;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.Platform;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;
using ViewModelMain = PixiEditor.ViewModels.ViewModelMain;
using ViewModels_ViewModelMain = PixiEditor.ViewModels.ViewModelMain;

namespace PixiEditor.Initialization;

internal class ClassicDesktopEntry
{
    private IClassicDesktopStyleApplicationLifetime desktop;

    public ClassicDesktopEntry(IClassicDesktopStyleApplicationLifetime desktop)
    {
        this.desktop = desktop;
        IActivatableLifetime? activable =
            (IActivatableLifetime?)App.Current.TryGetFeature(typeof(IActivatableLifetime));
        if (activable != null)
        {
            activable.Activated += ActivableOnActivated;
        }

        desktop.Startup += Start;
        desktop.ShutdownRequested += ShutdownRequested;
    }

    private void ActivableOnActivated(object? sender, ActivatedEventArgs e)
    {
        if (e.Kind == ActivationKind.File && e is FileActivatedEventArgs fileActivatedEventArgs)
        {
            IOperatingSystem.Current.HandleActivatedWithFile(fileActivatedEventArgs);
        }
        else if (e.Kind == ActivationKind.OpenUri && e is ProtocolActivatedEventArgs openUriEventArgs)
        {
            IOperatingSystem.Current.HandleActivatedWithUri(openUriEventArgs);
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

        desktop.MainWindow = new MainWindow(extensionLoader);
        desktop.MainWindow.Show();
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

        ExtensionLoader extensionLoader = new ExtensionLoader(Paths.ExtensionPackagesPath, Paths.UserExtensionsPath);
        //TODO: fetch from extension store
        extensionLoader.AddOfficialExtension("pixieditor.supporterpack",
            new OfficialExtensionData("supporter-pack.snk", AdditionalContentProduct.SupporterPack));
        extensionLoader.AddOfficialExtension("pixieditor.beta", new OfficialExtensionData());
        if (!safeMode)
        {
            extensionLoader.LoadExtensions();
        }

        return extensionLoader;
    }

    private IPlatform GetActivePlatform()
    {
#if STEAM || DEV_STEAM
        return new PixiEditor.Platform.Steam.SteamPlatform();
#elif MSIX || MSIX_DEBUG
        return new PixiEditor.Platform.MSStore.MicrosoftStorePlatform();
#else
        return new PixiEditor.Platform.Standalone.StandalonePlatform();
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

    private void ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        // TODO: Make sure this works
        var vm = ViewModels_ViewModelMain.Current;
        if (vm is null)
            return;

        if (vm.DocumentManagerSubViewModel.Documents.Any(x => !x.AllChangesSaved))
        {
            e.Cancel = true;
            Task.Run(async () =>
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    ConfirmationType confirmation = await ConfirmationDialog.Show(
                        new LocalizedString("SESSION_UNSAVED_DATA", "Shutdown"),
                        $"Shutdown");

                    if (confirmation != ConfirmationType.Yes)
                    {
                        desktop.Shutdown();
                    }
                });
            });
        }
    }
}
