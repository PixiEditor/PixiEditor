using System.Collections.ObjectModel;
using Avalonia.Controls;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Column : MultiChildLayoutElement
{
    public Column()
    {
    }

    public Column(params LayoutElement[] children)
    {
        Children = new ObservableCollection<LayoutElement>(children);
    }

    public override Control BuildNative()
    {
        StackPanel panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical
        };

        panel.Children.AddRange(Children.Select(x => x.BuildNative()));

        return panel;
    }
}
