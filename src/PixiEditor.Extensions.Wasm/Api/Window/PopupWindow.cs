using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Wasm.Async;

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
        get => Interop.get_window_title(windowHandle);
        set => Interop.set_window_title(windowHandle, value);
    }

    public void Show()
    {
        Interop.show_window(windowHandle);
    }

    public void Close()
    {
        Interop.close_window(windowHandle);
    }

    public AsyncCall ShowDialog()
    {
        int asyncHandle = Interop.show_window_async(windowHandle);
        AsyncCall showDialogTask = Interop.AsyncHandleToTask<int>(asyncHandle);
        return showDialogTask;
    }

    public double Width
    {
        get => Interop.get_window_width(windowHandle);
        set => Interop.set_window_width(windowHandle, value);
    }

    public double Height
    {
        get => Interop.get_window_height(windowHandle);
        set => Interop.set_window_height(windowHandle, value);
    }

    public bool CanResize
    {
        get => Interop.get_window_resizable(windowHandle);
        set => Interop.set_window_resizable(windowHandle, value);
    }

    public bool CanMinimize
    {
        get => Interop.get_window_minimizable(windowHandle);
        set => Interop.set_window_minimizable(windowHandle, value);
    }
    
    Task<bool?> IPopupWindow.ShowDialog()
    {
        throw new PlatformNotSupportedException("Task-based ShowDialog is not supproted in Wasm. Use PopupWindows.ShowDialog() with AsyncCall return type instead.");
    }
}
