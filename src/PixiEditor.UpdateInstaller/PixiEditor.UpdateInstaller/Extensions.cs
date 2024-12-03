using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace PixiEditor.UpdateInstaller;

public static class Extensions
{
    private const int MaxPath = 255;

    public static string GetExecutablePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            StringBuilder sb = new StringBuilder(MaxPath);
            GetModuleFileName(IntPtr.Zero, sb, MaxPath);
            return sb.ToString();
        }

        return Process.GetCurrentProcess().MainModule?.FileName;
    }

    [DllImport("kernel32.dll")]
    private static extern uint GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);
}
