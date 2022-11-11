using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace PixiEditor.Helpers;

internal static class ProcessHelper
{
    public static Process RunAsAdmin(string path)
    {
        var proc = new Process();
        try
        {
            proc.StartInfo.FileName = path;
            proc.StartInfo.Verb = "runas";
            proc.StartInfo.UseShellExecute = true;
            proc.Start();
        }
        catch (Win32Exception)
        {
            throw;
        }

        return proc;
    }

    public static bool IsRunningAsAdministrator()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void OpenInExplorer(string path)
    {
        string fixedPath = Path.GetFullPath(path);
        Process.Start("explorer.exe", $"/select,\"{fixedPath}\"");
    }
}
