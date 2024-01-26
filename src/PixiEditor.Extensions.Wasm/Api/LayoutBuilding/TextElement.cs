using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public abstract class TextElement(string value = "") : LayoutElement, ITextElement<NativeControl>
{
    public string Value { get; set; } = value;

    public abstract override NativeControl Build();
}
