using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using PixiEditor.Extensions.UI.Panels;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Column : MultiChildLayoutElement, IPropertyDeserializable
{
    private MainAxisAlignment mainAxisAlignment;
    private CrossAxisAlignment crossAxisAlignment;

    private Panel panel;

    public MainAxisAlignment MainAxisAlignment
    {
        get => mainAxisAlignment;
        set => SetField(ref mainAxisAlignment, value);
    }

    public CrossAxisAlignment CrossAxisAlignment
    {
        get => crossAxisAlignment;
        set => SetField(ref crossAxisAlignment, value);
    }

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

    protected override Control CreateNativeControl()
    {
        panel = new ColumnPanel() { MainAxisAlignment = MainAxisAlignment, CrossAxisAlignment = CrossAxisAlignment };

        panel.Children.AddRange(Children.Select(x => x.BuildNative()));

        return panel;
    }

    protected override IEnumerable<object> GetControlProperties()
    {
        yield return MainAxisAlignment;
        yield return CrossAxisAlignment;
    }

    protected override void DeserializeControlProperties(List<object> values)
    {
        if (values.Count < 2)
            return;

        int mainAxisAlignment = (int)values[0];
        int crossAxisAlignment = (int)values[1];

        MainAxisAlignment = (MainAxisAlignment)mainAxisAlignment;
        CrossAxisAlignment = (CrossAxisAlignment)crossAxisAlignment;
    }
}
