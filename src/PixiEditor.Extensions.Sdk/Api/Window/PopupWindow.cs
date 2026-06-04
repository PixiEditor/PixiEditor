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
        AsyncCall<bool?> showDialogTask = Native.CreateAsyncCall<bool?>(asyncHandle, ConvertWindowResult);
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
    
    private bool? ConvertWindowResult(byte[] result)
    {
        int bytesInInt = sizeof(int);
        if (result.Length != bytesInInt)        {
            throw new InvalidOperationException($"Expected result length of {bytesInInt} bytes, but got {result.Length} bytes.");
        }

        int resultValue = BitConverter.ToInt32(result, 0);

        if(resultValue == -1) return null;
        return resultValue == 1;
    }
}
