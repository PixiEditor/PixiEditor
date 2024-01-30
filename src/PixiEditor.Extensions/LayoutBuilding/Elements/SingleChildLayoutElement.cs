using System.Collections;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public abstract class SingleChildLayoutElement : LayoutElement, ISingleChildLayoutElement<Control>, IChildHost
{
    private ILayoutElement<Control>? _child;

    public ILayoutElement<Control>? Child
    {
        get => _child;
        set => SetField(ref _child, value);
    }

    public abstract override Control BuildNative();

    void IChildHost.DeserializeChildren(List<ILayoutElement<Control>> children)
    {
        Child = children.FirstOrDefault();
    }

    public void AddChild(ILayoutElement<Control> child)
    {
        Child = child;
    }

    public void RemoveChild(ILayoutElement<Control> child)
    {
        Child = null;
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
