using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Text : ITextElement<Control>, IDeserializable
{
    public string Data { get; set; }

    public Text(string data = "")
    {
        Data = data;
    }

    Control ILayoutElement<Control>.Build()
    {
        return new TextBlock { Text = Data };
    }

    void IDeserializable.DeserializeProperties(List<object> values)
    {
        Data = (string)values[0];
    }
}
