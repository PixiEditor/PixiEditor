using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public abstract class SingleChildLayoutElement : LayoutElement, ISingleChildLayoutElement<CompiledControl>
{
    public ILayoutElement<CompiledControl> Child { get; set; }
    public abstract override CompiledControl BuildNative();
}
