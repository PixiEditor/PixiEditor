namespace PixiEditor.Extensions.Windowing;

public interface IWindowProvider
{
    public PopupWindow CreatePopupWindow(string title, string bodyXaml);
}
