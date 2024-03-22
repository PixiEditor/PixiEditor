namespace PixiEditor.Extensions.Windowing;

public interface IPopupWindow
{
    public string? Title { get; set; }
    public void Show();
    public void Close();
    public Task<bool?> ShowDialog();
    public double Width { get; set; }
    public double Height { get; set; }
    public bool CanResize { get; set; }
    public bool CanMinimize { get; set; }
}
