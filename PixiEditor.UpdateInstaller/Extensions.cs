using System;

namespace PixiEditor.UpdateInstaller
{
    public static class Extensions
	{
		[System.Runtime.InteropServices.DllImport("kernel32.dll")]
		static extern uint GetModuleFileName(IntPtr hModule, System.Text.StringBuilder lpFilename, int nSize);
		static readonly int MAX_PATH = 255;
		public static string GetExecutablePath()
		{
			if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
			{
				var sb = new System.Text.StringBuilder(MAX_PATH);
				GetModuleFileName(IntPtr.Zero, sb, MAX_PATH);
				return sb.ToString();
			}
			else
			{
				return System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
			}
		}
	}
}
