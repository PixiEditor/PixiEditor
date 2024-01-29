using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.State;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public abstract class State : IState<CompiledControl>
{
    ILayoutElement<CompiledControl> IState<CompiledControl>.Build()
    {
        return BuildElement();
    }

    public abstract LayoutElement BuildElement();

    public void SetState(Action setAction)
    {
        setAction();
        StateChanged?.Invoke();
    }

    public event Action StateChanged;
}
