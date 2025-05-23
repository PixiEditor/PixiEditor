using System.Diagnostics;
using System.Net;
using System.Security;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Linux;

public class LinuxProcessUtility : IProcessUtility
{
    public Process RunAsAdmin(string path, string args)
    {
        // polkit 
        
        return Execute("pkexec", $"env DISPLAY=$DISPLAY XAUTHORITY=$XAUTHORITY {path} {args}");
    }

    public bool IsRunningAsAdministrator()
    {
        return Environment.IsPrivilegedProcess;
    }

    public Process ShellExecute(string toExecute)
    {
        Process process = new Process();
        process.StartInfo.FileName = toExecute;
        process.StartInfo.UseShellExecute = true;
        process.Start();
        
        return process;
    }

    public Process ShellExecute(string toExecute, string args)
    {
        Process process = new Process();
        process.StartInfo.FileName = toExecute;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = true;
        process.Start();
        
        return process;
    }

    public Process Execute(string path, string args)
    {
        Process process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.Start();
        
        return process;
    }
}
