using System.Collections;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class StatefulContainer : StatefulElement<ContainerState>, IChildHost
{
    public override ContainerState CreateState()
    {
         return new ContainerState();
    }

    void IChildHost.DeserializeChildren(List<ILayoutElement<Control>> children)
    {
        State.Content = (LayoutElement)children.FirstOrDefault();
    }

    public void AddChild(ILayoutElement<Control> child)
    {
        State.SetState(() => State.Content = (LayoutElement)child);
    }

    public void RemoveChild(ILayoutElement<Control> child)
    {
        State.SetState(() => State.Content = null);
    }

    public IEnumerator<ILayoutElement<Control>> GetEnumerator()
    {
        yield return State.Content;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
