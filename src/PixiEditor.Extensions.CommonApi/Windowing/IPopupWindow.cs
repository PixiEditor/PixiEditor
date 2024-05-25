using PixiEditor.Extensions.CommonApi.Async;

namespace PixiEditor.Extensions.CommonApi.Windowing;

public interface IPopupWindow
{
    public string? Title { get; set; }
    public void Show();
    public void Close();
    public AsyncCall<bool?> ShowDialog(); 
    public double Width { get; set; }
    public double Height { get; set; }
    public bool CanResize { get; set; }
    public bool CanMinimize { get; set; }
}
