using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Layout : SingleChildLayoutElement
{
    public Layout()
    {

    }

    public Layout(ILayoutElement<Control> body = null)
    {
        Child = body;
    }

    public override Control BuildNative()
    {
        Panel panel = new Panel();
        if (Child != null)
        {
            panel.Children.Add(Child.BuildNative());
        }

        return panel;
    }
}
