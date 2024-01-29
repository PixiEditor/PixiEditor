using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public abstract class SingleChildLayoutElement : LayoutElement, ISingleChildLayoutElement<Control>, IChildrenDeserializable
{
    public ILayoutElement<Control>? Child { get; set; }
    public abstract override Control BuildNative();

    void IChildrenDeserializable.DeserializeChildren(List<ILayoutElement<Control>> children)
    {
        Child = children.FirstOrDefault();
    }
}
