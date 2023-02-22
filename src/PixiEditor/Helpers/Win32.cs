using System.Runtime.InteropServices;
using System.Text;

namespace PixiEditor.Helpers;
internal class Win32
{
    public const int WM_GETMINMAXINFO = 0x0024;
    public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
    public const int WH_MOUSE_LL = 14;
    public const int WM_LBUTTONUP = 0x0202;
    public const int WM_MBUTTONUP = 0x0208;
    public const int WM_RBUTTONUP = 0x0205;

    public delegate int HookProc(int nCode, int wParam, IntPtr lParam);


    [StructLayout(LayoutKind.Sequential)]
    public struct MSLLHOOKSTRUCT
    {
        public POINT Pt;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    public enum MapType : uint
    {
        /// <summary>
        /// The uCode parameter is a virtual-key code and is translated into a scan code. If it is a virtual-key code that does not distinguish between left- and right-hand keys, the left-hand scan code is returned. If there is no translation, the function returns 0.
        /// </summary>
        MAPVK_VK_TO_VSC = 0x0,

        /// <summary>
        /// The uCode parameter is a scan code and is translated into a virtual-key code that does not distinguish between left- and right-hand keys. If there is no translation, the function returns 0.
        /// </summary>
        MAPVK_VSC_TO_VK = 0x1,

        /// <summary>
        /// The uCode parameter is a virtual-key code and is translated into an unshifted character value in the low order word of the return value. Dead keys (diacritics) are indicated by setting the top bit of the return value. If there is no translation, the function returns 0.
        /// </summary>
        MAPVK_VK_TO_CHAR = 0x2,

        /// <summary>
        /// The uCode parameter is a scan code and is translated into a virtual-key code that distinguishes between left- and right-hand keys. If there is no translation, the function returns 0.
        /// </summary>
        MAPVK_VSC_TO_VK_EX = 0x3,
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }


    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr handle, uint flags);

    [DllImport("user32.dll")]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    [DllImport("user32.dll")]
    public static extern int ToUnicode(
        uint wVirtKey,
        uint wScanCode,
        byte[] lpKeyState,
        [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
        StringBuilder pwszBuff,
        int cchBuff,
        uint wFlags);

    [DllImport("user32.dll")]
    public static extern bool GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    public static extern uint MapVirtualKeyExW(uint uCode, MapType uMapType, int hkl);

    [DllImport(
        "user32.dll",
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
    public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

    [DllImport(
        "user32.dll",
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
    public static extern int UnhookWindowsHookEx(int idHook);

    [DllImport(
        "user32.dll",
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(string name);
}

