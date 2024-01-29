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
}
