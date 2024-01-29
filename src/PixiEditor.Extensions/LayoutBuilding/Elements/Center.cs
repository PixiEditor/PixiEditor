using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Center : SingleChildLayoutElement, IPropertyDeserializable
{
    public Center(ILayoutElement<Control> child = null)
    {
        Child = child;
    }

    public override Control BuildNative()
    {
        return new Panel()
        {
            Children =
            {
                Child.BuildNative()
            },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
    }

    void IPropertyDeserializable.DeserializeProperties(List<object> values)
    {

    }
}
