using System.Diagnostics;
using PixiEditor.OperatingSystem;

namespace PixiEditor.MacOs;

internal class MacOsProcessUtility : IProcessUtility
{
    public Process RunAsAdmin(string path)
    {
        return RunAsAdmin(path, true);
    }

    public Process RunAsAdmin(string path, bool createWindow)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = path,
            Verb = "runas",
            UseShellExecute = createWindow,
            CreateNoWindow = !createWindow,
            RedirectStandardOutput = !createWindow,
            RedirectStandardError = !createWindow,
            WindowStyle = createWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden
        };

        Process p = new Process();
        p.StartInfo = startInfo;

        p.Start();
        return p;
    }

    public bool IsRunningAsAdministrator()
    {
        return Environment.IsPrivilegedProcess;
    }

    public static void ShellExecute(string url)
    {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true, });
    }

    void IProcessUtility.ShellExecute(string url)
    {
        ShellExecute(url);
    }
    
    void IProcessUtility.ShellExecute(string url, string args)
    {
        ShellExecute(url, args);
    }

    public static void ShellExecute(string url, string args)
    {
        Process.Start(new ProcessStartInfo { FileName = url, Arguments = args, UseShellExecute = true, });
    }

    public static void ShellExecuteEV(string path) => ShellExecute(Environment.ExpandEnvironmentVariables(path));

    public static void ShellExecuteEV(string path, string args) =>
        ShellExecute(Environment.ExpandEnvironmentVariables(path), args);
}
