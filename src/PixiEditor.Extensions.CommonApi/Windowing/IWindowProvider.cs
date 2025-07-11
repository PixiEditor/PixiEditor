using System.ComponentModel;

namespace PixiEditor.Extensions.CommonApi.Windowing;

public interface IWindowProvider
{
    public IPopupWindow CreatePopupWindow(string title, object body);
    public IPopupWindow GetWindow(BuiltInWindowType type);
    public IPopupWindow GetWindow(string windowId);
    public void SubscribeWindowOpened(BuiltInWindowType type, Action<IPopupWindow> action);
}

public enum BuiltInWindowType
{
    [Description("PalettesBrowser")]
    PalettesBrowser,
    [Description("HelloTherePopup")]
    StartupWindow,
    [Description("LoginPopup")]
    AccountManagement,
}
