using System.Diagnostics;
using System.Runtime.InteropServices;
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
            UseShellExecute = true,
        });
    }

    void IProcessUtility.ShellExecute(string url)
    {
        ShellExecute(url);
    }

    public static void ShellExecute(string url, string args)
    {
        Process.Start(new ProcessStartInfo
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
