using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.Ui;
using PixiEditor.Extensions.CommonApi.Windowing;
using PixiEditor.Extensions.Sdk.Api.FlyUI;
using PixiEditor.Extensions.Sdk.Api.Window;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Ui;

public class VisualTreeProvider : IVisualTreeProvider
{
    ILayoutElement<T>? IVisualTreeProvider.FindElement<T>(string name)
    {
        return FindElement(name) as ILayoutElement<T>;
    }

    ILayoutElement<T> IVisualTreeProvider.FindElement<T>(string name, IPopupWindow root)
    {
        return FindElement(name, root as PopupWindow) as ILayoutElement<T>;
    }

    public LayoutElement? FindElement(string name)
    {
        return Interop.FindUiElement(name);
    }

    public LayoutElement? FindElement(string name, PopupWindow root)
    {
        if (root == null)
        {
            throw new ArgumentNullException(nameof(root), "Root window cannot be null.");
        }

        return Interop.FindUiElement(name, root);
    }
}
