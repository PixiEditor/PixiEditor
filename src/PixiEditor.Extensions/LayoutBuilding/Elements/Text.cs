using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Text : ITextElement<Control>, IPropertyDeserializable
{
    public string Value { get; set; }

    public Text(string value = "")
    {
        Value = value;
    }

    Control ILayoutElement<Control>.Build()
    {
        return new TextBlock { Text = Value };
    }

    void IPropertyDeserializable.DeserializeProperties(List<object> values)
    {
        Value = (string)values[0];
    }
}
