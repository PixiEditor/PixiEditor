using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Column : MultiChildLayoutElement
{
    private DockPanel panel;

    public Column()
    {
    }

    public Column(params LayoutElement[] children)
    {
        Children = new ObservableCollection<LayoutElement>(children);
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
                    DockPanel.SetDock(newChild, Dock.Top);
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
        panel = new DockPanel
        {
            LastChildFill = true,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        panel.Children.AddRange(Children.Select(x => x.BuildNative()));

        foreach (var child in panel.Children)
        {
            DockPanel.SetDock(child, Dock.Top);
        }

        return panel;
    }
}
