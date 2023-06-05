namespace PixiEditor.Extensions;

public interface IPopupWindow
{
    public string UniqueId { get; }
    public string Title { get; set; }
    public void Show();
    public void Close();
    public bool? ShowDialog();
    public double Width { get; set; }
    public double Height { get; set; }
}
