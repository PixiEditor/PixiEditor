using System.Runtime.InteropServices;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PixiEditor.Helpers;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Windows;

public sealed class WindowsOperatingSystem : IOperatingSystem
{
    public string Name => "Windows";
    
    public string AnalyticsId => "Windows";
    
    public IInputKeys InputKeys { get; } = new WindowsInputKeys();
    public IProcessUtility ProcessUtility { get; } = new WindowsProcessUtility();
    public ISecureStorage SecureStorage { get; } = new WindowsSecureStorage();

    private const string UniqueEventName = "33f1410b-2ad7-412a-a468-34fe0a85747c";
    
    private const string UniqueMutexName = "ab2afe27-b9ee-4f03-a1e4-c18da16a349c";
    
    private EventWaitHandle _eventWaitHandle;
    
    private Mutex _mutex;
    
    private string passedArgsFile = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PixiEditor", ".passedArgs");

    public string ExecutableExtension { get; } = ".exe";

    public void OpenUri(string uri)
    {
        WindowsProcessUtility.ShellExecute(uri);
    }

    public void OpenFolder(string path)
    {
        string dirName = Path.GetDirectoryName(path);
        string fileName = Path.GetFileName(path);

        if (dirName == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(fileName))
        {
            WindowsProcessUtility.SelectInFileExplorer(path);
            return;
        }

        WindowsProcessUtility.ShellExecuteEV(dirName);
    }

    public bool HandleNewInstance(Dispatcher? dispatcher, Action<string, bool> openInExistingAction, IApplicationLifetime lifetime)
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
                        dispatcher.Invoke(() => openInExistingAction(passedArgsFile, false));
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
        (lifetime as IClassicDesktopStyleApplicationLifetime)!.Shutdown();
        return false;
    }

    public void HandleActivatedWithFile(FileActivatedEventArgs fileActivatedEventArgs) { }

    public void HandleActivatedWithUri(ProtocolActivatedEventArgs openUriEventArgs) { }

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
}
