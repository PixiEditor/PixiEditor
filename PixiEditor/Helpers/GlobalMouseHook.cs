using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;

namespace PixiEditor.Helpers
{
    public delegate void MouseUpEventHandler(object sender, Point p);

    // see https://stackoverflow.com/questions/22659925/how-to-capture-mouseup-event-outside-the-wpf-window
    [ExcludeFromCodeCoverage]
    public static class GlobalMouseHook
    {
        private const int WhMouseLl = 14;
        private const int WmLbuttonup = 0x0202;
        private static int mouseHookHandle;
        private static HookProc mouseDelegate;

        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);

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

        private static event MouseUpEventHandler MouseUp;

        public static void RaiseMouseUp()
        {
            MouseUp?.Invoke(default, default);
        }

        private static void Unsubscribe()
        {
            if (mouseHookHandle != 0)
            {
                int result = UnhookWindowsHookEx(mouseHookHandle);
                mouseHookHandle = 0;
                mouseDelegate = null;
                if (result == 0)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode);
                }
            }
        }

        private static void Subscribe()
        {
            if (mouseHookHandle == 0)
            {
                mouseDelegate = MouseHookProc;
                mouseHookHandle = SetWindowsHookEx(
                    WhMouseLl,
                    mouseDelegate,
                    GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
                    0);
                if (mouseHookHandle == 0)
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
                Msllhookstruct mouseHookStruct = (Msllhookstruct)Marshal.PtrToStructure(lParam, typeof(Msllhookstruct));
                if (wParam == WmLbuttonup)
                {
                    if (MouseUp != null)
                    {
                        MouseUp.Invoke(null, new System.Windows.Point(mouseHookStruct.Pt.X, mouseHookStruct.Pt.Y));
                    }
                }
            }

            return CallNextHookEx(mouseHookHandle, nCode, wParam, lParam);
        }

        [DllImport(
            "user32.dll",
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

        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            public readonly int X;
            public readonly int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Msllhookstruct
        {
            public readonly Point Pt;
            public readonly uint MouseData;
            public readonly uint Flags;
            public readonly uint Time;
            public readonly IntPtr DwExtraInfo;
        }
    }
}