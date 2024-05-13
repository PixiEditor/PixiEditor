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
        get => Interop.GetWindowTitle(windowHandle);
        set => Interop.SetWindowTitle(windowHandle, value);
    }

    public void Show()
    {
        Interop.ShowWindow(windowHandle);
    }

    public void Close()
    {
        Interop.CloseWindow(windowHandle);
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
