using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public sealed class Layout : ISingleChildLayoutElement<Control>, IPropertyDeserializable
{
    public ILayoutElement<Control> Child { get; set; }

    public Layout(ILayoutElement<Control> body = null)
    {
        Child = body;
    }

    public Control Build()
    {
        return new Panel { Children = { Child.Build() } };
    }

    void IPropertyDeserializable.DeserializeProperties(List<object> values)
    {

    }
}
