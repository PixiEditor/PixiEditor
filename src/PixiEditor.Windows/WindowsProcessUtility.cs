using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Windows;

public class WindowsProcessUtility : IProcessUtility
{
    public Process RunAsAdmin(string path, string args)
    {
        return RunAsAdmin(path, args, true);
    }

    public Process RunAsAdmin(string path, string args, bool createWindow)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = path,
            Verb = "runas",
            Arguments = args,
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
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static Process ShellExecute(string url)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true,
        });
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
        return Process.Start(new ProcessStartInfo
        {
            FileName = url,
            Arguments = args,
            UseShellExecute = true,
        });
    }

    public static void ShellExecuteEV(string path) => ShellExecute(Environment.ExpandEnvironmentVariables(path));

    public static void ShellExecuteEV(string path, string args) => ShellExecute(Environment.ExpandEnvironmentVariables(path), args);

    public static void SelectInFileExplorer(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            throw new ArgumentNullException("fullPath");

        fullPath = Path.GetFullPath(fullPath);

        IntPtr pidlList = NativeMethods.ILCreateFromPathW(fullPath);
        if (pidlList != IntPtr.Zero)
        {
            try
            {
                // Open parent folder and select item
                Marshal.ThrowExceptionForHR(NativeMethods.SHOpenFolderAndSelectItems(pidlList, 0, IntPtr.Zero, 0));
            }
            finally
            {
                NativeMethods.ILFree(pidlList);
            }
        }
    }

    static class NativeMethods
    {
        [DllImport("shell32.dll", ExactSpelling = true)]
        public static extern void ILFree(IntPtr pidlList);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern IntPtr ILCreateFromPathW(string pszPath);

        [DllImport("shell32.dll", ExactSpelling = true)]
        public static extern int SHOpenFolderAndSelectItems(IntPtr pidlList, uint cild, IntPtr children, uint dwFlags);
    }
}
