using System.ComponentModel;

namespace PixiEditor.Extensions.Windowing;

public interface IWindowProvider
{
    public PopupWindow CreatePopupWindow(string title, object body);
    public PopupWindow GetWindow(WindowType type);
    public PopupWindow GetWindow(string windowId);
}

public enum WindowType
{
    [Description("PalettesBrowser")]
    PalettesBrowser
}
