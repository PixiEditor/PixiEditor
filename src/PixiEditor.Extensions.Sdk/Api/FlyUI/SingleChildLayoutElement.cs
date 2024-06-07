using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public abstract class SingleChildLayoutElement : LayoutElement, ISingleChildLayoutElement<CompiledControl>
{
    public ILayoutElement<CompiledControl> Child { get; set; }
    public abstract override CompiledControl BuildNative();
}
