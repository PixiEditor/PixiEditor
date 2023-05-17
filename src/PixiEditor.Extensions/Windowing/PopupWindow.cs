namespace PixiEditor.Extensions.Windowing;

public class PopupWindow : IPopupWindow
{
    private IPopupWindow _underlyingWindow;

    public PopupWindow(IPopupWindow basicPopup)
    {
        _underlyingWindow = basicPopup;
    }

    public string Title
    {
        get => _underlyingWindow.Title;
        set => _underlyingWindow.Title = value;
    }

    public void Show() => _underlyingWindow.Show();

    public bool? ShowDialog() => _underlyingWindow.ShowDialog();
}
