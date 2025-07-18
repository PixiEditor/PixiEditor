using System.Collections;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.FlyUI.Elements.Native;

public class NativeMultiChildElement : NativeElement, IChildHost, IMultiChildLayoutElement<Control>
{
    private Panel nativePanel => (Panel)Native;

    public NativeMultiChildElement(Panel native) : base(native)
    {
    }

    List<ILayoutElement<Control>> IMultiChildLayoutElement<Control>.Children
    {
        get => Children.Cast<ILayoutElement<Control>>().ToList();
        set => throw new NotSupportedException("Setting children directly is not supported for NativeMultiChildElement.");
    }

    public ObservableCollection<LayoutElement> Children => WrapPanelChildren();

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
        throw new NotSupportedException("Deserializing children directly is not supported for NativeMultiChildElement.");
    }

    public void AddChild(ILayoutElement<Control> child)
    {
        Children.Add((LayoutElement)child);
    }

    public void RemoveChild(ILayoutElement<Control> child)
    {
        Children.Remove((LayoutElement)child);
    }

    public void AppendChild(int atIndex, ILayoutElement<Control> deserializedChild)
    {
        if (atIndex < 0 || atIndex > Children.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(atIndex), "Index is out of range.");
        }

        LayoutElement layoutChild = (LayoutElement)deserializedChild;

        nativePanel.Children.Insert(atIndex, layoutChild.BuildNative());
        Children.Insert(atIndex, layoutChild);
    }

    private ObservableCollection<LayoutElement> WrapPanelChildren()
    {
        ObservableCollection<LayoutElement> wrappedChildren = new();
        foreach (var child in nativePanel.Children)
        {
            NativeElement nativeChild = new NativeElement(child);
            wrappedChildren.Add(nativeChild);
        }

        return wrappedChildren;
    }
}
