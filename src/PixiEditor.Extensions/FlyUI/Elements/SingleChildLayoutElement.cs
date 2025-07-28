using System.Collections;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.FlyUI.Elements;

public abstract class SingleChildLayoutElement : LayoutElement, ISingleChildLayoutElement<Control>, IChildHost
{
    private LayoutElement _child;

    ILayoutElement<Control>? ISingleChildLayoutElement<Control>.Child
    {
        get => Child;
        set => Child = (LayoutElement)value;
    }

    public LayoutElement Child
    {
        get => _child;
        set
        {
            SetField(ref _child, value);
        }
    }

    protected abstract void AddChild(Control child);
    protected abstract void RemoveChild();

    void IChildHost.DeserializeChildren(List<ILayoutElement<Control>> children)
    {
        Child = (LayoutElement)children.FirstOrDefault();
    }

    public void AddChild(ILayoutElement<Control> child)
    {
        Child = (LayoutElement)child;
        AddChild(child.BuildNative());
    }

    public void RemoveChild(ILayoutElement<Control> child)
    {
        Child = null;
        RemoveChild();
    }

    public void AppendChild(int atIndex, ILayoutElement<Control> deserializedChild)
    {
        if (atIndex != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(atIndex), "SingleChildLayoutElement can only have one child at index 0.");
        }

        Child = (LayoutElement)deserializedChild;
        AddChild(deserializedChild.BuildNative());
    }

    public IEnumerator<ILayoutElement<Control>> GetEnumerator()
    {
        if (Child != null)
        {
            yield return Child;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
