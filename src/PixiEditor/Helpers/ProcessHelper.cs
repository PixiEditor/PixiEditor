using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Input;
using PixiEditor.Exceptions;

namespace PixiEditor.Helpers;

internal static class ProcessHelper
{
    public static Process RunAsAdmin(string path)
    {
        var proc = new Process();
        proc.StartInfo.FileName = path;
        proc.StartInfo.Verb = "runas";
        proc.StartInfo.UseShellExecute = true;
        proc.Start();

        return proc;
    }

    public static bool IsRunningAsAdministrator()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void OpenInExplorer(string path)
    {
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
            string fixedPath = Path.GetFullPath(path);
            var process = Process.Start("explorer.exe", $"/select,\"{fixedPath}\"");
            // Explorer might need a second to show up
            process.WaitForExit(500);
        }
        catch (Win32Exception)
        {
            throw new RecoverableException("ERROR_FAILED_TO_OPEN_EXPLORER");
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }
}
