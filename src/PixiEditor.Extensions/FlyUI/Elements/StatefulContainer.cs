using System.Collections;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.FlyUI.Elements;

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

    public void AppendChild(int atIndex, ILayoutElement<Control> deserializedChild)
    {
        if (atIndex != 0)
        {
            throw new NotSupportedException("Appending children at an index other than 0 is not supported for StatefulContainer.");
        }

        State.SetState(() => State.Content = (LayoutElement)deserializedChild);
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
