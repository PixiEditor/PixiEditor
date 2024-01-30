using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public interface IChildHost : IEnumerable<ILayoutElement<Control>>
{
    public void DeserializeChildren(List<ILayoutElement<Control>> children);
    public void AddChild(ILayoutElement<Control> child);
    public void RemoveChild(ILayoutElement<Control> child);
}
