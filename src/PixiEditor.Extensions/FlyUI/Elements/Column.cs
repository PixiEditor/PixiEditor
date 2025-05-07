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

    public override Control BuildNative()
    {
        panel = new ColumnPanel() { MainAxisAlignment = MainAxisAlignment, CrossAxisAlignment = CrossAxisAlignment };

        panel.Children.AddRange(Children.Select(x => x.BuildNative()));

        return panel;
    }

    public IEnumerable<object> GetProperties()
    {
        yield return MainAxisAlignment;
        yield return CrossAxisAlignment;
    }

    public void DeserializeProperties(ImmutableList<object> values)
    {
        if (values.Count < 2)
            throw new ArgumentException("Invalid number of properties");

        int mainAxisAlignment = (int)values[0];
        int crossAxisAlignment = (int)values[1];

        MainAxisAlignment = (MainAxisAlignment)mainAxisAlignment;
        CrossAxisAlignment = (CrossAxisAlignment)crossAxisAlignment;
    }
}
