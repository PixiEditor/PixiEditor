namespace PixiEditor.Extensions;

public interface IPopupWindow
{
    public string Title { get; set; }
    public void Show();
    public bool? ShowDialog();
    public double Width { get; set; }
    public double Height { get; set; }
}
