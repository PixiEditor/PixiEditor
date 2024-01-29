using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public interface IChildrenDeserializable
{
    public void DeserializeChildren(List<ILayoutElement<Control>> children);
}
