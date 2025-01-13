using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PixiEditor.OperatingSystem;

namespace PixiEditor.MacOs;

public sealed class MacOperatingSystem : IOperatingSystem
{
    public string Name { get; } = "MacOS";

    public string AnalyticsId => "macOS";
    
    public IInputKeys InputKeys { get; } = new MacOsInputKeys();
    public IProcessUtility ProcessUtility { get; } = new MacOsProcessUtility();
    public void OpenUri(string uri)
    {
        ProcessUtility.ShellExecute(uri);
    }

    public void OpenFolder(string path)
    {
        ProcessUtility.ShellExecute(Path.GetDirectoryName(path));
    }

    public bool HandleNewInstance(Dispatcher? dispatcher, Action<string> openInExistingAction, IApplicationLifetime lifetime)
    {
        return true;
    }
}
