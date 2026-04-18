using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Threading;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Wrap : MultiChildLayoutElement
{
    private ItemAlignment alignment;
    private double spacing;
    private double runSpacing;
    private Axis direction;

    private WrapPanel panel;

    public ItemAlignment Alignment
    {
        get => alignment;
        set => SetField(ref alignment, value);
    }

    public Axis Direction
    {
        get => direction;
        set => SetField(ref direction, value);
    }

    public double Spacing
    {
        get => spacing;
        set => SetField(ref spacing, value);
    }

    public double RunSpacing
    {
        get => runSpacing;
        set => SetField(ref runSpacing, value);
    }

    public Wrap()
    {
    }

    public Wrap(params LayoutElement[] children)
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

    protected override Control CreateNativeControl()
    {
        panel = new WrapPanel()
        {
            ItemsAlignment = (WrapPanelItemsAlignment)Alignment,
        };

        panel.Children.AddRange(Children.Select(x => x.BuildNative()));

        return panel;
    }

    protected override IEnumerable<object> GetControlProperties()
    {
        yield return Alignment;
        yield return Direction;
        yield return RunSpacing;
        yield return Spacing;
    }

    protected override void DeserializeControlProperties(List<object> values)
    {
        if (values.Count < 2)
            return;

        Alignment = (ItemAlignment)values[0];
        Direction = (Axis)values[1];
        RunSpacing = (double)values[2];
        Spacing = (double)values[3];
    }
}
