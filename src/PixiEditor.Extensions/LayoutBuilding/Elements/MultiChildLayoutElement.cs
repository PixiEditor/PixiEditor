using System.Collections;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public abstract class MultiChildLayoutElement : LayoutElement, IMultiChildLayoutElement<Control>, IChildHost
{
    private ObservableCollection<LayoutElement> children = new();

    List<ILayoutElement<Control>> IMultiChildLayoutElement<Control>.Children
    {
        get => Children.Cast<ILayoutElement<Control>>().ToList();
        set => DeserializeChildren(value);
    }

    public ObservableCollection<LayoutElement> Children
    {
        get => children;
        set
        {
            children = value;
            OnPropertyChanged();
        }
    }

    public abstract override Control BuildNative();

    public IEnumerator<ILayoutElement<Control>> GetEnumerator()
    {
        return Children.ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void DeserializeChildren(List<ILayoutElement<Control>> children)
    {
        Children = new ObservableCollection<LayoutElement>(children.Cast<LayoutElement>());
    }

    public void AddChild(ILayoutElement<Control> child)
    {
        Children.Add((LayoutElement)child);
    }

    public void RemoveChild(ILayoutElement<Control> child)
    {
        Children.Remove((LayoutElement)child);
    }
}
