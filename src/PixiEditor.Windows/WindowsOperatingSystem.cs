using PixiEditor.Helpers;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Windows;

public class WindowsOperatingSystem : IOperatingSystem
{
    public string Name => "Windows";
    public IInputKeys InputKeys { get; } = new WindowsInputKeys();
    public IProcessUtility ProcessUtility { get; } = new WindowsProcessUtility();

    public WindowsOperatingSystem() => IOperatingSystem.RegisterOS(this);

    public void OpenHyperlink(string url)
    {
        WindowsProcessUtility.ShellExecute(url);
    }

    public void OpenFolder(string path)
    {
        WindowsProcessUtility.ShellExecuteEV(path);
    }
}
