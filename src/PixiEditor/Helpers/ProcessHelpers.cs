using System.Diagnostics;

namespace PixiEditor.Helpers;

internal static class ProcessHelpers
{
    public static void ShellExecute(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public static void ShellExecuteEV(string path) => ShellExecute(Environment.ExpandEnvironmentVariables(path));
}
