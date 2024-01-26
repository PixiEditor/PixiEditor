using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public abstract class SingleChildLayoutElement : LayoutElement, ISingleChildLayoutElement<NativeControl>
{
    public ILayoutElement<NativeControl> Child { get; set; }
    public abstract override NativeControl Build();
}
