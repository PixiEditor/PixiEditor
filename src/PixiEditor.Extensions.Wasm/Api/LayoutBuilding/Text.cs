using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Text : ITextElement<NativeControl>
{
    public string Data { get; set; }

    public Text(string data)
    {
        Data = data;
    }

    NativeControl ILayoutElement<NativeControl>.Build()
    {
        NativeControl text = new NativeControl("Text");
        text.AddProperty(Data);
        return text;
    }
}
