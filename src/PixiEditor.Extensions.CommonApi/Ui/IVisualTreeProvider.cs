using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.Windowing;

namespace PixiEditor.Extensions.CommonApi.Ui;

public interface IVisualTreeProvider
{
    public ILayoutElement<T>? FindElement<T>(string name);
    public ILayoutElement<T>? FindElement<T>(string name, IPopupWindow root);
}
