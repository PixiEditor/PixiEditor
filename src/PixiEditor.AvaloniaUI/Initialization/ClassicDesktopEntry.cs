using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.Controllers;
using PixiEditor.AvaloniaUI.Models.ExceptionHandling;
using PixiEditor.AvaloniaUI.Models.IO;
using PixiEditor.AvaloniaUI.Views;
using PixiEditor.AvaloniaUI.Views.Dialogs;
using PixiEditor.Extensions.Runtime;
using PixiEditor.OperatingSystem;
using PixiEditor.Platform;
using PixiEditor.Windows;
using ViewModelMain = PixiEditor.AvaloniaUI.ViewModels.ViewModelMain;

namespace PixiEditor.AvaloniaUI.Initialization;

internal class ClassicDesktopEntry
{
        /// <summary>The event mutex name.</summary>
    private const string UniqueEventName = "33f1410b-2ad7-412a-a468-34fe0a85747c";

    /// <summary>The unique mutex name.</summary>
    private const string UniqueMutexName = "ab2afe27-b9ee-4f03-a1e4-c18da16a349c";

    /// <summary>The event wait handle.</summary>
    private EventWaitHandle _eventWaitHandle;

    private string passedArgsFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PixiEditor", ".passedArgs");
    private IClassicDesktopStyleApplicationLifetime desktop;

    public ClassicDesktopEntry(IClassicDesktopStyleApplicationLifetime desktop)
    {
        this.desktop = desktop;
        desktop.Startup += Start;
    }

    /// <summary>The mutex.</summary>
    private Mutex _mutex;

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

        #if !STEAM
        if (!HandleNewInstance(dispatcher))
        {
            return;
        }
        #endif

        InitOperatingSystem();
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
        InitPlatform();

        ExtensionLoader extensionLoader = new ExtensionLoader(Paths.ExtensionsFullPath);
        //TODO: fetch from extension store
        extensionLoader.AddOfficialExtension("pixieditor.supporterpack", new OfficialExtensionData("supporter-pack.snk", AdditionalContentProduct.SupporterPack));
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
        var os = GetActiveOperatingSystem();
    }

    private IOperatingSystem GetActiveOperatingSystem()
    {
        return new WindowsOperatingSystem();
    }

    private bool HandleNewInstance(Dispatcher? dispatcher)
    {
        bool isOwned;
        _mutex = new Mutex(true, UniqueMutexName, out isOwned);
        _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

        GC.KeepAlive(_mutex);

        if (dispatcher == null)
            return true;

        if (isOwned)
        {
            var thread = new Thread(
                () =>
                {
                    while (_eventWaitHandle.WaitOne())
                    {
                        dispatcher.Invoke(
                            (Action)(() =>
                            {
                                if (desktop.MainWindow is MainWindow mainWindow)
                                {
                                    mainWindow.BringIntoView();
                                    List<string> args = new List<string>();
                                    if (File.Exists(passedArgsFile))
                                    {
                                        args = CommandLineHelpers.SplitCommandLine(File.ReadAllText(passedArgsFile)).ToList();
                                        File.Delete(passedArgsFile);
                                    }

                                    StartupArgs.Args = args;
                                    StartupArgs.Args.Add("--openedInExisting");
                                    ViewModelMain viewModel = (ViewModelMain)mainWindow.DataContext;
                                    viewModel.StartupCommand.Execute(null);
                                }
                            }));
                    }
                })
            {
                // It is important mark it as background otherwise it will prevent app from exiting.
                IsBackground = true
            };

            thread.Start();
            return true;
        }

        // Notify other instance so it could bring itself to foreground.
        File.WriteAllText(passedArgsFile, string.Join(' ', WrapSpaces(Environment.GetCommandLineArgs())));
        _eventWaitHandle.Set();

        // Terminate this instance.
        desktop.Shutdown();
        return false;
    }

    private string?[] WrapSpaces(string[] args)
    {
        string?[] wrappedArgs = new string?[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg.Contains(' '))
            {
                wrappedArgs[i] = $"\"{arg}\"";
            }
            else
            {
                wrappedArgs[i] = arg;
            }
        }

        return wrappedArgs;
    }

    //TODO: Implement this
    /*protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
        base.OnSessionEnding(e);

        var vm = ViewModelMain.Current;
        if (vm is null)
            return;

        if (vm.DocumentManagerSubViewModel.Documents.Any(x => !x.AllChangesSaved))
        {
            ConfirmationType confirmation = ConfirmationDialog.Show(
                new LocalizedString("SESSION_UNSAVED_DATA", e.ReasonSessionEnding),
                $"{e.ReasonSessionEnding}");
            e.Cancel = confirmation != ConfirmationType.Yes;
        }
    }*/

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
}
