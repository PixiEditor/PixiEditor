using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public abstract class SingleChildLayoutElement : LayoutElement, ISingleChildLayoutElement<CompiledControl>
{
    public ILayoutElement<CompiledControl> Child { get; set; }
    public abstract override CompiledControl BuildNative();
}
