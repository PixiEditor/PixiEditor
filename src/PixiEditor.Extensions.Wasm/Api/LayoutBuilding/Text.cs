using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Text : ITextElement<NativeControl>
{
    public string Value { get; set; }

    public Text(string value)
    {
        Value = value;
    }

    NativeControl ILayoutElement<NativeControl>.Build()
    {
        NativeControl text = new NativeControl("Text");
        text.AddProperty(Value);
        return text;
    }
}
