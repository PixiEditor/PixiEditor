using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Threading;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Row : MultiChildLayoutElement
{
    private StackPanel panel;
    public Row()
    {
    }

    public Row(params LayoutElement[] children)
    {
        Children = new(children);
        Children.CollectionChanged += ChildrenOnCollectionChanged;
    }

    private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (LayoutElement? item in e.NewItems)
                {
                    var newChild = item.BuildNative();
                    panel.Children.Add(newChild);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (LayoutElement? item in e.OldItems)
                {
                    panel.Children.RemoveAt(e.OldStartingIndex);
                }
            }
        });
    }

    public override Control BuildNative()
    {
        panel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal
        };

        panel.Children.AddRange(Children.Select(x => x.BuildNative()));

        return panel;
    }
}
