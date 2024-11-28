using System.Diagnostics;
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
}
