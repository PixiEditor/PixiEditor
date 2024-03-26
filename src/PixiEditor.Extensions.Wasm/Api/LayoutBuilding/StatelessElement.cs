using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.State;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public abstract class StatelessElement : LayoutElement, IStatelessElement<CompiledControl>
{
    public ILayoutElement<CompiledControl> Build()
    {
        return this;
    }
}
