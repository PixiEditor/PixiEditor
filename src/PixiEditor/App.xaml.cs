using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.AppExtensions;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Platform;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;
using Timer = System.Timers.Timer;

namespace PixiEditor;

internal partial class App : Application
{
    /// <summary>The event mutex name.</summary>
    private const string UniqueEventName = "33f1410b-2ad7-412a-a468-34fe0a85747c";

    /// <summary>The unique mutex name.</summary>
    private const string UniqueMutexName = "ab2afe27-b9ee-4f03-a1e4-c18da16a349c";

    /// <summary>The event wait handle.</summary>
    private EventWaitHandle _eventWaitHandle;

    private string passedArgsFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PixiEditor", ".passedArgs");

    /// <summary>The mutex.</summary>
    private Mutex _mutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        StartupArgs.Args = e.Args.ToList();
        string arguments = string.Join(' ', e.Args);

        if (ParseArgument("--crash (\"?)([A-z0-9:\\/\\ -_.]+)\\1", arguments, out Group[] groups))
        {
            CrashReport report = CrashReport.Parse(groups[2].Value);
            MainWindow = new CrashReportDialog(report);
            MainWindow.Show();
            return;
        }

        #if !STEAM
        if (!HandleNewInstance())
        {
            return;
        }
        #endif

        LoadingWindow.ShowInNewThread();

        AddNativeAssets();

        InitPlatform();

        ExtensionLoader extensionLoader = new ExtensionLoader();
        extensionLoader.LoadExtensions();

        MainWindow = new MainWindow(extensionLoader);
        MainWindow.ContentRendered += (_, _) =>
        {
            LoadingWindow.Instance.SafeClose();
            MainWindow.Activate();
        };
        MainWindow.Show();
    }

    private void InitPlatform()
    {
        var platform = GetActivePlatform();
        IPlatform.RegisterPlatform(platform);
        platform.PerformHandshake();
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

    private void AddNativeAssets()
    {
        var iconFont = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000)
            ? "Segoe Fluent Icons"
            : "Segoe MDL2 Assets";
        
        Resources.Add("NativeIconFont", new FontFamily(iconFont));
    }

    private bool HandleNewInstance()
    {
        bool isOwned;
        _mutex = new Mutex(true, UniqueMutexName, out isOwned);
        _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

        GC.KeepAlive(_mutex);

        if (Current?.Dispatcher == null)
            return true;

        if (isOwned)
        {
            var thread = new Thread(
                () =>
                {
                    while (_eventWaitHandle.WaitOne())
                    {
                        Current.Dispatcher.BeginInvoke(
                            (Action)(() =>
                            {
                                MainWindow mainWindow = ((MainWindow)Current.MainWindow);
                                if (mainWindow != null)
                                {
                                    mainWindow.BringToForeground();
                                    List<string> args = new List<string>();
                                    if (File.Exists(passedArgsFile))
                                    {
                                        args = CommandLineHelpers.SplitCommandLine(File.ReadAllText(passedArgsFile)).ToList();
                                        File.Delete(passedArgsFile);
                                    }
                                    
                                    StartupArgs.Args = args;
                                    StartupArgs.Args.Add("--openedInExisting");
                                    mainWindow.DataContext.OnStartupCommand.Execute(null);
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
        Shutdown();
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

    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
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
}
