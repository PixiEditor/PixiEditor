using Avalonia.Controls;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Row : MultiChildLayoutElement
{
    public Row()
    {
    }

    public Row(params LayoutElement[] children)
    {
        Children = new(children);
    }
    public override Control BuildNative()
    {
        StackPanel panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal
        };

        panel.Children.AddRange(Children.Select(x => x.BuildNative()));

        return panel;
    }
}
