using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Text : TextElement, IPropertyDeserializable
{
    public Text(string value = "")
    {
        Value = value;
    }

    public override Control Build()
    {
        return new TextBlock { Text = Value };
    }

    void IPropertyDeserializable.DeserializeProperties(List<object> values)
    {
        Value = (string)values[0];
    }
}
