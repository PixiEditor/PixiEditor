using Avalonia.Controls;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Text : StatelessElement, IPropertyDeserializable
{
    public string Value { get; set; }
    public Text(string value = "")
    {
        Value = value;
    }

    public override Control BuildNative()
    {
        return new TextBlock { Text = Value };
    }

    void IPropertyDeserializable.DeserializeProperties(List<object> values)
    {
        Value = (string)values[0];
    }
}
