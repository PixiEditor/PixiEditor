using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Wasm.Async;
using PixiEditor.Extensions.Wasm.Bridge;

namespace PixiEditor.Extensions.Wasm.Api.Window;

public class PopupWindow : IPopupWindow
{
    private int windowHandle;

    internal PopupWindow(int handle)
    {
        windowHandle = handle;
    }

    public string Title
    {
        get => Native.get_window_title(windowHandle);
        set => Native.set_window_title(windowHandle, value);
    }

    public void Show()
    {
        Native.show_window(windowHandle);
    }

    public void Close()
    {
        Native.close_window(windowHandle);
    }

    public AsyncCall ShowDialog()
    {
        int asyncHandle = Native.show_window_async(windowHandle);
        AsyncCall showDialogTask = Native.AsyncHandleToTask<int>(asyncHandle);
        return showDialogTask;
    }

    public double Width
    {
        get => Native.get_window_width(windowHandle);
        set => Native.set_window_width(windowHandle, value);
    }

    public double Height
    {
        get => Native.get_window_height(windowHandle);
        set => Native.set_window_height(windowHandle, value);
    }

    public bool CanResize
    {
        get => Native.get_window_resizable(windowHandle);
        set => Native.set_window_resizable(windowHandle, value);
    }

    public bool CanMinimize
    {
        get => Native.get_window_minimizable(windowHandle);
        set => Native.set_window_minimizable(windowHandle, value);
    }
    
    Task<bool?> IPopupWindow.ShowDialog()
    {
        throw new PlatformNotSupportedException("Task-based ShowDialog is not supproted in Wasm. Use PopupWindows.ShowDialog() with AsyncCall return type instead.");
    }
}
