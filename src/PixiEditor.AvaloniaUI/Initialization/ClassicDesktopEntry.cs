using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Controllers;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Models.ExceptionHandling;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.Views;
using PixiEditor.AvaloniaUI.Views.Dialogs;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Runtime;
using PixiEditor.OperatingSystem;
using PixiEditor.Platform;
using ViewModelMain = PixiEditor.AvaloniaUI.ViewModels.ViewModelMain;

namespace PixiEditor.AvaloniaUI.Initialization;

internal class ClassicDesktopEntry
{
    private IClassicDesktopStyleApplicationLifetime desktop;

    public ClassicDesktopEntry(IClassicDesktopStyleApplicationLifetime desktop)
    {
        this.desktop = desktop;
        desktop.Startup += Start;
        desktop.ShutdownRequested += ShutdownRequested;
    }

    private void Start(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        StartupArgs.Args = e.Args.ToList();
        string arguments = string.Join(' ', e.Args);

        if (ParseArgument("--crash (\"?)([A-z0-9:\\/\\ -_.]+)\\1", arguments, out Group[] groups))
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
                    CrashHelper.SendExceptionInfoToWebhook(exception, true);
                }
                finally
                {
                    // TODO: find an avalonia replacement for messagebox 
                    //MessageBox.Show("Fatal error", $"Fatal error while trying to open crash report in App.OnStartup()\n{exception}");
                }
            }

            return;
        }

        Dispatcher dispatcher = Dispatcher.UIThread;
        InitOperatingSystem();

#if !STEAM
        if (!HandleNewInstance(dispatcher))
        {
            return;
        }
#endif

        var extensionLoader = InitApp();

        desktop.MainWindow = new MainWindow(extensionLoader);
        desktop.MainWindow.Show();
    }

    private void InitPlatform()
    {
        var platform = GetActivePlatform();
        IPlatform.RegisterPlatform(platform);
        platform.PerformHandshake();
    }

    public ExtensionLoader InitApp()
    {
        LoadingWindow.ShowInNewThread();

        InitPlatform();

        ExtensionLoader extensionLoader = new ExtensionLoader(Paths.ExtensionPackagesPath, Paths.UserExtensionsPath);
        //TODO: fetch from extension store
        extensionLoader.AddOfficialExtension("pixieditor.supporterpack",
            new OfficialExtensionData("supporter-pack.snk", AdditionalContentProduct.SupporterPack));
        extensionLoader.AddOfficialExtension("pixieditor.closedbeta1", new OfficialExtensionData());
        extensionLoader.LoadExtensions();

        return extensionLoader;
    }

    private IPlatform GetActivePlatform()
    {
#if STEAM
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

    private void OpenInExisting(string passedArgsFile)
    {
        if (desktop.MainWindow is MainWindow mainWindow)
        {
            mainWindow.BringIntoView();
            List<string> args = new List<string>();
            if (File.Exists(passedArgsFile))
            {
                args = CommandLineHelpers.SplitCommandLine(File.ReadAllText(passedArgsFile))
                    .ToList();
                File.Delete(passedArgsFile);
            }

            StartupArgs.Args = args;
            StartupArgs.Args.Add("--openedInExisting");
            ViewModelMain viewModel = (ViewModelMain)mainWindow.DataContext;
            viewModel.StartupCommand.Execute(null);
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
        var vm = ViewModelMain.Current;
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
