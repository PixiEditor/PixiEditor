using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Input;
using Hardware.Info;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Helpers;

internal static class ProcessHelper
{
    public static Process RunAsAdmin(string path)
    {
        return IOperatingSystem.Current.ProcessUtility.RunAsAdmin(path);
    }

    public static bool IsRunningAsAdministrator()
    {
        return IOperatingSystem.Current.ProcessUtility.IsRunningAsAdministrator();
    }

    public static void OpenInExplorer(string path)
    {
        try
        {
            string fixedPath = Path.GetFullPath(path);
            var process = Process.Start("explorer.exe", $"/select,\"{fixedPath}\"");

            // Explorer might need a second to show up
            process.WaitForExit(500);
        }
        finally{}
    }
}
