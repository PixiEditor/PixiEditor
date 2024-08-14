using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Linux;

public sealed class LinuxOperatingSystem : IOperatingSystem
{
    public string Name { get; } = "Linux";
    public IInputKeys InputKeys { get; } = new LinuxInputKeys();
    public IProcessUtility ProcessUtility { get; }
    
    public void OpenUri(string uri)
    {
        return;
    }

    public void OpenFolder(string path)
    {
        return;
    }

    public bool HandleNewInstance(Dispatcher? dispatcher, Action<string> openInExistingAction, IApplicationLifetime lifetime)
    {
        return true;
    }

    class LinuxInputKeys : IInputKeys
    {
        public string GetKeyboardKey(Key key, bool forceInvariant = false) => $"{key}";
    }
}
