using Avalonia.Controls;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Layout : SingleChildLayoutElement
{
    private Panel panel;
    public Layout()
    {

    }

    public Layout(LayoutElement body = null)
    {
        Child = body;
    }

    protected override Control CreateNativeControl()
    {
        panel = new Panel();
        if (Child != null)
        {
            panel.Children.Add(Child.BuildNative());
        }

        return panel;
    }

    protected override void AddChild(Control child)
    {
        panel.Children.Add(child);
    }

    protected override void RemoveChild()
    {
        panel.Children.Clear();
    }
}
