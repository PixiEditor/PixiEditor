using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.FlyUI.Elements;

public interface IChildHost : IEnumerable<ILayoutElement<Control>>
{
    public void DeserializeChildren(List<ILayoutElement<Control>> children);
    public void AddChild(ILayoutElement<Control> child);
    public void RemoveChild(ILayoutElement<Control> child);
    public void AppendChild(int atIndex, ILayoutElement<Control> deserializedChild);
}
