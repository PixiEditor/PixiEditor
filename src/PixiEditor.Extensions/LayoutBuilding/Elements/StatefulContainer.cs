using System.Collections;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class StatefulContainer : StatefulElement<ContainerState>, IChildrenDeserializable
{
    public override ContainerState CreateState()
    {
        return new();
    }

    void IChildrenDeserializable.DeserializeChildren(List<ILayoutElement<Control>> children)
    {
        State.Content = children.FirstOrDefault();
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
