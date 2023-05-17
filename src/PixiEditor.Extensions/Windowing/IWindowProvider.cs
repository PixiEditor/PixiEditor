using System.Windows.Controls;

namespace PixiEditor.Extensions.Windowing;

public interface IWindowProvider
{
    public PopupWindow CreatePopupWindow(string title, object body);
}
