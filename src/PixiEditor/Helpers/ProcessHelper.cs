using System.Diagnostics;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Helpers;

internal static class ProcessHelper
{
    public static Process RunAsAdmin(string path, string? args = null)
    {
        if (IOperatingSystem.Current.IsLinux)
        {
            return IOperatingSystem.Current.ProcessUtility.ShellExecute(path, args);
        }
        
        return IOperatingSystem.Current.ProcessUtility.RunAsAdmin(path, args);
    }

    public static bool IsRunningAsAdministrator()
    {
        return IOperatingSystem.Current.ProcessUtility.IsRunningAsAdministrator();
    }
}
