using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Text : TextElement
{
    public Text(string value)
    {
        Value = value;
    }

    public override NativeControl Build()
    {
        NativeControl text = new NativeControl("Text");
        text.AddProperty(Value);
        return text;
    }
}
