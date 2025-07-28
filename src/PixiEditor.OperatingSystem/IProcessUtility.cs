using System.Diagnostics;

namespace PixiEditor.OperatingSystem;

public interface IProcessUtility
{
    public Process RunAsAdmin(string path, string? args);
    public bool IsRunningAsAdministrator();
    public Process ShellExecute(string toExecute);
    public Process ShellExecute(string toExecute, string? args);
    public Process Execute(string path, string args);
}
