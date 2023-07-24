using System.Diagnostics;
using System.Security.Principal;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Windows;

public class WindowsProcessUtility : IProcessUtility
{
    public Process RunAsAdmin(string path)
    {
        var proc = new Process();
        proc.StartInfo.FileName = path;
        proc.StartInfo.Verb = "runas";
        proc.StartInfo.UseShellExecute = true;
        proc.Start();

        return proc;
    }

    public bool IsRunningAsAdministrator()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void ShellExecute(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public static void ShellExecuteEV(string path) => ShellExecute(Environment.ExpandEnvironmentVariables(path));
}
