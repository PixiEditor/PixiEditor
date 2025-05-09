using System.ComponentModel;
using Avalonia.Controls;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Center : SingleChildLayoutElement
{
    private Panel panel;
    public Center()
    {

    }

    public Center(LayoutElement child = null)
    {
        Child = child;
    }

    protected override Control CreateNativeControl()
    {
        panel = new Panel()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        if (Child != null)
        {
            Control child = Child.BuildNative();
            panel.Children.Add(child);
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
