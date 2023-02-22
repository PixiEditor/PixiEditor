using System;
using System.Runtime.InteropServices;

namespace PixiEditor.Helpers;

static class WindowSizeHelper
{
    public static IntPtr SetMaxSizeHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // All windows messages (msg) can be found here
        // https://docs.microsoft.com/de-de/windows/win32/winmsg/window-notifications
        if (msg == Win32.WM_GETMINMAXINFO)
        {
            // We need to tell the system what our size should be when maximized. Otherwise it will
            // cover the whole screen, including the task bar.
            Win32.MINMAXINFO mmi = (Win32.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(Win32.MINMAXINFO));

            // Adjust the maximized size and position to fit the work area of the correct monitor
            IntPtr monitor = Win32.MonitorFromWindow(hwnd, Win32.MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                Win32.MONITORINFO monitorInfo = default;
                monitorInfo.cbSize = Marshal.SizeOf(typeof(Win32.MONITORINFO));
                Win32.GetMonitorInfo(monitor, ref monitorInfo);
                Win32.RECT rcWorkArea = monitorInfo.rcWork;
                Win32.RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.X = Math.Abs(rcWorkArea.Left - rcMonitorArea.Left);
                mmi.ptMaxPosition.Y = Math.Abs(rcWorkArea.Top - rcMonitorArea.Top);
                mmi.ptMaxSize.X = Math.Abs(rcWorkArea.Right - rcWorkArea.Left);
                mmi.ptMaxSize.Y = Math.Abs(rcWorkArea.Bottom - rcWorkArea.Top);
            }

            Marshal.StructureToPtr(mmi, lParam, true);
        }

        return IntPtr.Zero;
    }
}
