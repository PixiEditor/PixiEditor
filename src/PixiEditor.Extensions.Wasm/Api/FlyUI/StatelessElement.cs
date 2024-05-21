using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.State;

namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public abstract class StatelessElement : LayoutElement, IStatelessElement<CompiledControl>
{
    public ILayoutElement<CompiledControl> Build()
    {
        return this;
    }
}
