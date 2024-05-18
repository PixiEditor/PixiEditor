using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PixiEditor.OperatingSystem;

namespace PixiEditor.MacOs;

public sealed class MacOperatingSystem : IOperatingSystem
{
    public string Name { get; } = "MacOS";
    public IInputKeys InputKeys { get; }
    public IProcessUtility ProcessUtility { get; }
    public void OpenUri(string uri)
    {
        throw new NotImplementedException();
    }

    public void OpenFolder(string path)
    {
        throw new NotImplementedException();
    }

    public bool HandleNewInstance(Dispatcher? dispatcher, Action<string> openInExistingAction, IApplicationLifetime lifetime)
    {
        throw new NotImplementedException();
    }
}
