using System.Runtime.InteropServices;
using PixiEditor.Helpers;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Windows;

public sealed class WindowsOperatingSystem : IOperatingSystem
{
    public string Name => "Windows";
    public IInputKeys InputKeys { get; } = new WindowsInputKeys();
    public IProcessUtility ProcessUtility { get; } = new WindowsProcessUtility();

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
}
