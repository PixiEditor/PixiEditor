using PixiEditor.Extensions.CommonApi.Windowing;

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

    public Task<bool?> ShowDialog()
    {
        throw new NotImplementedException();
    }

    public double Width { get; set; }
    public double Height { get; set; }
    public bool CanResize { get; set; }
    public bool CanMinimize { get; set; }
}
