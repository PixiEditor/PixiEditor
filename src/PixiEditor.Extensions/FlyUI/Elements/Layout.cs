using Avalonia.Controls;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Layout : SingleChildLayoutElement
{
    public Layout()
    {

    }

    public Layout(LayoutElement body = null)
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
