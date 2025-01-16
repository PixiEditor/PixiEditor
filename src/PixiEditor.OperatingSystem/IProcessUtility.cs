using System.Diagnostics;

namespace PixiEditor.OperatingSystem;

public interface IProcessUtility
{
    public Process RunAsAdmin(string path);
    public Process RunAsAdmin(string path, bool createWindow);
    public bool IsRunningAsAdministrator();
    public void ShellExecute(string toExecute);
    public void ShellExecute(string toExecute, string args);
}
