using System.Diagnostics;
using PixiEditor.OperatingSystem;

namespace PixiEditor.MacOs;

internal class MacOsProcessUtility : IProcessUtility
{
    public Process RunAsAdmin(string path, string args)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = path,
            Verb = "runas",
            Arguments = args,
            UseShellExecute = true
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

    public static Process ShellExecute(string url)
    {
        return Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true, });
    }

    Process IProcessUtility.ShellExecute(string url)
    {
        return ShellExecute(url);
    }

    Process IProcessUtility.ShellExecute(string url, string args)
    {
        return ShellExecute(url, args);
    }

    Process IProcessUtility.Execute(string path, string args)
    {
        return Execute(path, args);
    }

    public static Process Execute(string path, string args)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = path,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });
    }

    public static Process ShellExecute(string url, string args)
    {
        return Process.Start(new ProcessStartInfo { FileName = url, Arguments = args, UseShellExecute = true });
    }

    public static void ShellExecuteEV(string path) => ShellExecute(Environment.ExpandEnvironmentVariables(path));

    public static void ShellExecuteEV(string path, string args) =>
        ShellExecute(Environment.ExpandEnvironmentVariables(path), args);
}
