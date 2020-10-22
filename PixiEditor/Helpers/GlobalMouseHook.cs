using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Helpers
{
    // see https://stackoverflow.com/questions/22659925/how-to-capture-mouseup-event-outside-the-wpf-window
    [ExcludeFromCodeCoverage]
    public static class GlobalMouseHook
    {
        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        private static int _mouseHookHandle;
        private static HookProc _mouseDelegate;

        private static event MouseUpEventHandler MouseUp;
        public static event MouseUpEventHandler OnMouseUp
        {
            add
            {
                Subscribe();
                MouseUp += value;
            }
            remove
            {
                MouseUp -= value;
                Unsubscribe();
            }
        }

        public static void RaiseMouseUp()
        {
            MouseUp?.Invoke(default, default, default);
        }

        private static void Unsubscribe()
        {
            if (_mouseHookHandle != 0)
            {
                int result = UnhookWindowsHookEx(_mouseHookHandle);
                _mouseHookHandle = 0;
                _mouseDelegate = null;
                if (result == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private static void Subscribe()
        {
            if (_mouseHookHandle == 0)
            {
                _mouseDelegate = MouseHookProc;
                _mouseHookHandle = SetWindowsHookEx(
                    WH_MOUSE_LL,
                    _mouseDelegate,
                    GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
                    0);
                if (_mouseHookHandle == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private static int MouseHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT mouseHookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                if (wParam == WM_LBUTTONUP || wParam == WM_MBUTTONUP || wParam == WM_RBUTTONUP)
                {
                    if (MouseUp != null)
                    {
                        MouseButton button = wParam == WM_LBUTTONUP ? MouseButton.Left
                            : wParam == WM_MBUTTONUP ? MouseButton.Middle : MouseButton.Right;
                        MouseUp.Invoke(null, new Point(mouseHookStruct.pt.x, mouseHookStruct.pt.y), button);
                    }
                }
            }
            return CallNextHookEx(_mouseHookHandle, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_MBUTTONUP = 0x0208;
        private const int WM_RBUTTONUP = 0x0205;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", 
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall, 
            SetLastError = true)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

        [DllImport(
            "user32.dll",
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall,
            SetLastError = true)]
        private static extern int UnhookWindowsHookEx(int idHook);

        [DllImport(
            "user32.dll", 
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string name);
    }

    public delegate void MouseUpEventHandler(object sender, Point p, MouseButton button);
}
