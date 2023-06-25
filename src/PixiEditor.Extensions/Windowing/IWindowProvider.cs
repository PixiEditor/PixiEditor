namespace PixiEditor.Extensions.Windowing;

public interface IWindowProvider
{
    public PopupWindow CreatePopupWindow(string title, object body);
    public PopupWindow OpenWindow(WindowType type);
    public PopupWindow OpenWindow(string windowId);
}

public enum WindowType
{
    BrowserPalette
}
