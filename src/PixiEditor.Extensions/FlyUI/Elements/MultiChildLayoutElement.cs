using System.Collections;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.FlyUI.Elements;

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
            SetField(ref children, value);
        }
    }

    /*public abstract void AddChild(Control child);
    public abstract void RemoveChild(int atIndex);*/

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
        int index = Children.IndexOf((LayoutElement)child);
        Children.Remove((LayoutElement)child);
    }

    public void AppendChild(int atIndex, ILayoutElement<Control> deserializedChild)
    {
        if (atIndex < 0 || atIndex > Children.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(atIndex), "Index is out of range.");
        }

        if (deserializedChild is not LayoutElement layoutChild)
        {
            throw new InvalidOperationException("Deserialized child must be of type LayoutElement.");
        }

        Children.Insert(atIndex, layoutChild);
    }
}
