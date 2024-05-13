using System.ComponentModel;

namespace PixiEditor.Extensions.CommonApi.Windowing;

public interface IWindowProvider
{
    public IPopupWindow CreatePopupWindow(string title, object body);
    public IPopupWindow GetWindow(WindowType type);
    public IPopupWindow GetWindow(string windowId);
}

public enum WindowType
{
    [Description("PalettesBrowser")]
    PalettesBrowser
}
