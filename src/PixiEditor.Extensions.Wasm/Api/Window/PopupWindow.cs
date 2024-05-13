using PixiEditor.Extensions.CommonApi.Windowing;

namespace PixiEditor.Extensions.Wasm.Api.Window;

public class PopupWindow : IPopupWindow
{
    public string Title { get; set; }
    public void Show()
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
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
