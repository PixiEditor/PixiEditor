using PixiEditor.Extensions.CommonApi.Async;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Window;

public class PopupWindow : IPopupWindow
{
    internal int Handle => windowHandle;
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

    public AsyncCall<bool?> ShowDialog()
    {
        int asyncHandle = Native.show_window_async(windowHandle);
        AsyncCall<bool?> showDialogTask = Native.CreateAsyncCall<bool?, int>(asyncHandle, ConvertWindowResult);
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
    
    private bool? ConvertWindowResult(int result)
    {
        if(result == -1) return null;
        return result == 1;
    }
}
