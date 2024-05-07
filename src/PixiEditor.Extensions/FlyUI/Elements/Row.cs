using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Threading;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Row : MultiChildLayoutElement
{
    private DockPanel panel;
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
                    DockPanel.SetDock(newChild, Dock.Left);
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
        panel = new DockPanel()
        {
            LastChildFill = true,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };

        panel.Children.AddRange(Children.Select(x => x.BuildNative()));

        foreach (var child in panel.Children)
        {
            DockPanel.SetDock(child, Dock.Left);
        }

        return panel;
    }
}
