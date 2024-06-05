using PixiEditor.Extensions.CommonApi.FlyUI.State;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public abstract class StatefulElement<TState> : LayoutElement, IStatefulElement<CompiledControl, TState> where TState : IState<CompiledControl>
{
    private TState state;

    IState<CompiledControl> IStatefulElement<CompiledControl>.State
    {
        get
        {
            if (state == null)
            {
                state = CreateState();
                state.StateChanged += () =>
                {
                    CompiledControl newLayout = BuildNative();
                    WasmExtension.Api.WindowProvider.LayoutStateChanged(UniqueId, newLayout);
                };
            }

            return state;
        }
    }

    public TState State => (TState)((IStatefulElement<CompiledControl>)this).State;

    public override CompiledControl BuildNative()
    {
        CompiledControl control = State.Build().BuildNative();
        CompiledControl statefulContainer = new CompiledControl(UniqueId, "StatefulContainer");
        statefulContainer.Children.Add(control);
        return statefulContainer;
    }

    public abstract TState CreateState();
}
