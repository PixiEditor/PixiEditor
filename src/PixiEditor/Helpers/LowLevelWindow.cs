using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PixiEditor.Helpers;
internal class LowLevelWindow
{
    private bool disposed;
    private Win32.WndProc wndProcDelegate;

    public IntPtr WindowHandle { get; private set; }

    public LowLevelWindow(string uniqueWindowName, IntPtr parentWindow)
    {
        if (string.IsNullOrEmpty(uniqueWindowName))
            throw new ArgumentException(nameof(uniqueWindowName));

        wndProcDelegate = CustomWndProc;

        // Create WNDCLASS
        Win32.WNDCLASS windowParams = new Win32.WNDCLASS();
        windowParams.lpszClassName = uniqueWindowName;
        windowParams.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);

        ushort classAtom = Win32.RegisterClassW(ref windowParams);

        int lastError = Marshal.GetLastWin32Error();
        if (classAtom == 0 && lastError != Win32.ERROR_CLASS_ALREADY_EXISTS)
            throw new Win32Exception("Could not register window class");

        // Create window
        WindowHandle = Win32.CreateWindowExW(
            0,
            uniqueWindowName,
            String.Empty,
            Win32.WS_CHILD, //| Win32.WS_OVERLAPPEDWINDOW
            0,
            0,
            0,
            0,
            parentWindow,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero
        );

        if (WindowHandle == 0)
            throw new Win32Exception("Could not create window");

        //Win32.ShowWindow(WindowHandle, 1);
        //Win32.UpdateWindow(WindowHandle);
    }

    public void RunEventLoop()
    {
        while (true)
        {
            var bRet = Win32.GetMessage(out Win32.MSG msg, WindowHandle, 0, 0);
            if (bRet == 0 || msg.message == Win32.WM_CLOSE)
                return;

            if (bRet == -1)
            {
                // handle the error and possibly exit
            }
            else
            {
                Win32.TranslateMessage(ref msg);
                Win32.DispatchMessage(ref msg);
            }
        }
    }

    private static IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        return Win32.DefWindowProcW(hWnd, msg, wParam, lParam);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
            }

            // Dispose unmanaged resources
            if (WindowHandle != IntPtr.Zero)
            {
                Win32.DestroyWindow(WindowHandle);
                WindowHandle = IntPtr.Zero;
            }
            disposed = true;
        }
    }

    ~LowLevelWindow() => Dispose(false);
}
