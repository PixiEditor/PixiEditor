using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Center : ISingleChildLayoutElement<Control>, IDeserializable
{
    public ILayoutElement<Control> Child { get; set; }

    public Center(ILayoutElement<Control> child = null)
    {
        Child = child;
    }

    Control ILayoutElement<Control>.Build()
    {
        return new Panel()
        {
            Children =
            {
                Child.Build()
            },
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
    }

    void IDeserializable.DeserializeProperties(List<object> values)
    {

    }
}
