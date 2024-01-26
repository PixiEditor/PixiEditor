using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public sealed class Layout : SingleChildLayoutElement, IPropertyDeserializable
{
    public Layout(ILayoutElement<Control> body = null)
    {
        Child = body;
    }

    public override Control Build()
    {
        Panel panel = new Panel();
        if (Child != null)
        {
            panel.Children.Add(Child.Build());
        }

        return panel;
    }

    void IPropertyDeserializable.DeserializeProperties(List<object> values)
    {

    }
}
