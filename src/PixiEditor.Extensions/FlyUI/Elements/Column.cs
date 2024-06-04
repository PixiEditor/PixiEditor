using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Column : MultiChildLayoutElement
{
    private StackPanel panel;

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
        panel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        
        panel.Children.AddRange(Children.Select(x => x.BuildNative()));

        return panel;
    }
}
