using System.Diagnostics;

namespace PixiEditor.OperatingSystem;

public interface IProcessUtility
{
    public Process RunAsAdmin(string path);
    public bool IsRunningAsAdministrator();
    public void ShellExecute(string toExecute);
}
