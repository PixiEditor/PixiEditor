using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using PixiEditor.Views;

namespace PixiEditor.Helpers;

public delegate void MouseUpEventHandler(object sender, Point p, MouseButton button);

// see https://stackoverflow.com/questions/22659925/how-to-capture-mouseup-event-outside-the-wpf-window
[ExcludeFromCodeCoverage]
internal class GlobalMouseHook
{
    private static readonly Lazy<GlobalMouseHook> lazy = new Lazy<GlobalMouseHook>(() => new GlobalMouseHook());
    public static GlobalMouseHook Instance => lazy.Value;

    public event MouseUpEventHandler OnMouseUp;

    private int mouseHookHandle;
    private Win32.HookProc mouseDelegate;

    private Thread mouseHookWindowThread;
    private IntPtr mainWindowHandle;
    private IntPtr childWindowHandle;

    private GlobalMouseHook() { }

    public void Initilize(MainWindow window)
    {
        // disable low-level hook in debug to prevent mouse lag when pausing in debugger
#if DEBUG
        return;
#endif
        mainWindowHandle = new WindowInteropHelper(window).Handle;
        if (mainWindowHandle == IntPtr.Zero)
            throw new InvalidOperationException();

        window.Closed += (_, _) =>
        {
            if (childWindowHandle != IntPtr.Zero)
                Win32.PostMessage(childWindowHandle, Win32.WM_CLOSE, 0, 0);
        };

        mouseHookWindowThread = new Thread(StartMouseHook)
        {
            Name = $"{nameof(GlobalMouseHook)} Thread"
        };
        mouseHookWindowThread.Start();
    }

    private void StartMouseHook()
    {
        LowLevelWindow window = new LowLevelWindow(nameof(GlobalMouseHook), mainWindowHandle);
        childWindowHandle = window.WindowHandle;

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

        window.RunEventLoop();
    }

    //private void Unsubscribe()
    //{
    //    int result = Win32.UnhookWindowsHookEx(mouseHookHandle);
    //    mouseHookHandle = 0;
    //    mouseDelegate = null;
    //    if (result == 0)
    //    {
    //        int errorCode = Marshal.GetLastWin32Error();
    //        throw new Win32Exception(errorCode);
    //    }
    //}

    private int MouseHookProc(int nCode, int wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            Win32.MSLLHOOKSTRUCT mouseHookStruct = (Win32.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.MSLLHOOKSTRUCT));
            if (wParam == Win32.WM_LBUTTONUP || wParam == Win32.WM_MBUTTONUP || wParam == Win32.WM_RBUTTONUP)
            {
                if (OnMouseUp is not null)
                {

                    MouseButton button = wParam == Win32.WM_LBUTTONUP ? MouseButton.Left
                        : wParam == Win32.WM_MBUTTONUP ? MouseButton.Middle : MouseButton.Right;
                    Application.Current?.Dispatcher.BeginInvoke(() =>
                        OnMouseUp.Invoke(null, new Point(mouseHookStruct.Pt.X, mouseHookStruct.Pt.Y), button));
                }
            }
        }

        return Win32.CallNextHookEx(mouseHookHandle, nCode, wParam, lParam);
    }
}
