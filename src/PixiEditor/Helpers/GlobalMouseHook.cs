using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PixiEditor.Helpers;

public delegate void MouseUpEventHandler(object sender, Point p, MouseButton button);

// see https://stackoverflow.com/questions/22659925/how-to-capture-mouseup-event-outside-the-wpf-window
[ExcludeFromCodeCoverage]
internal static class GlobalMouseHook
{
    private static int mouseHookHandle;
    private static Win32.HookProc mouseDelegate;


    public static event MouseUpEventHandler OnMouseUp
    {
        add
        {
            // disable low-level hook in debug to prevent mouse lag when pausing in debugger
#if !DEBUG
                Subscribe();
#endif
            MouseUp += value;
        }

        remove
        {
            MouseUp -= value;
#if !DEBUG
                Unsubscribe();
#endif
        }
    }

    private static event MouseUpEventHandler MouseUp;

    public static void RaiseMouseUp()
    {
        MouseUp?.Invoke(default, default, default);
    }

    private static void Unsubscribe()
    {
        if (mouseHookHandle != 0)
        {
            int result = Win32.UnhookWindowsHookEx(mouseHookHandle);
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
            mouseHookHandle = Win32.SetWindowsHookEx(
                Win32.WH_MOUSE_LL,
                mouseDelegate,
                Win32.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName),
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
            Win32.MSLLHOOKSTRUCT mouseHookStruct = (Win32.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.MSLLHOOKSTRUCT));
            if (wParam == Win32.WM_LBUTTONUP || wParam == Win32.WM_MBUTTONUP || wParam == Win32.WM_RBUTTONUP)
            {
                if (MouseUp != null)
                {

                    MouseButton button = wParam == Win32.WM_LBUTTONUP ? MouseButton.Left
                        : wParam == Win32.WM_MBUTTONUP ? MouseButton.Middle : MouseButton.Right;
                    Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                        MouseUp.Invoke(null, new Point(mouseHookStruct.Pt.X, mouseHookStruct.Pt.Y), button));
                }
            }
        }

        return Win32.CallNextHookEx(mouseHookHandle, nCode, wParam, lParam);
    }
}
