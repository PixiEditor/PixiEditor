using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public abstract class TextElement(string value = "") : LayoutElement, ITextElement<CompiledControl>
{
    public string Value { get; set; } = value;

    public abstract override CompiledControl Build();
}
