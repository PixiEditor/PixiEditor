using System.Text;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using DeviceId;
using PixiEditor.OperatingSystem;
using PixiEditor.OperatingSystem.Cryptography;

namespace PixiEditor.MacOs;

public sealed class MacOperatingSystem : IOperatingSystem
{
    public string Name { get; } = "MacOS";

    public string AnalyticsId => "macOS";
    
    public IInputKeys InputKeys { get; } = new MacOsInputKeys();
    public IProcessUtility ProcessUtility { get; } = new MacOsProcessUtility();

    public IEncryptor Encryptor { get; } = new AesHmacEncryptor(
        new MacDeviceIdBuilder(new DeviceIdBuilder().AddMachineName()).AddSystemDriveSerialNumber()
            .AddPlatformSerialNumber().ToString());

    private List<Uri> activationUris;

    public string ExecutableExtension { get; } = string.Empty;

    public void OpenUri(string uri)
    {
        ProcessUtility.ShellExecute(uri);
    }

    public void OpenFolder(string path)
    {
        ProcessUtility.ShellExecute(Path.GetDirectoryName(path));
    }

    public bool HandleNewInstance(Dispatcher? dispatcher, Action<string, bool> openInExistingAction, IApplicationLifetime lifetime)
    {
        StringBuilder args = new StringBuilder();
        
        if(activationUris != null)
        {
            foreach (var uri in activationUris)
            {
                args.Append('"');
                args.Append(uri.AbsolutePath);
                args.Append('"');
                args.Append(' ');
            }
        }
        
        dispatcher.Invoke(() => openInExistingAction(args.ToString(), true));
        return true;
    }

    public void HandleActivatedWithFile(FileActivatedEventArgs fileActivatedEventArgs)
    {
        if(activationUris == null)
        {
            activationUris = [];
        }
        
        foreach (var file in fileActivatedEventArgs.Files)
        {
           activationUris.Add(file.Path);
        }
    }

    public void HandleActivatedWithUri(ProtocolActivatedEventArgs openUriEventArgs)
    {
        if(activationUris == null)
        {
            activationUris = [];
        }
        
        activationUris.Add(openUriEventArgs.Uri);
    }
}
