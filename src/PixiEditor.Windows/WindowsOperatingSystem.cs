using System.Runtime.InteropServices;
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
}
