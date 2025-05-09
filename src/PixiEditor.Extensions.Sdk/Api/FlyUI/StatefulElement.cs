using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.State;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public abstract class StatefulElement<TState> : LayoutElement, IStatefulElement<ControlDefinition, TState> where TState : IState<ControlDefinition>
{
    private TState state;

    protected StatefulElement(Cursor? cursor) : base(cursor)
    {
    }

    IState<ControlDefinition> IStatefulElement<ControlDefinition>.State
    {
        get
        {
            if (state == null)
            {
                state = CreateState();
                state.StateChanged += () =>
                {
                    ControlDefinition newLayout = BuildNative();
                    PixiEditorExtension.Api.WindowProvider.LayoutStateChanged(UniqueId, newLayout);
                };
            }

            return state;
        }
    }

    public TState State => (TState)((IStatefulElement<ControlDefinition>)this).State;

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition controlDefinition = State.Build().BuildNative();
        ControlDefinition statefulContainer = new ControlDefinition(UniqueId, "StatefulContainer");
        statefulContainer.Children.Add(controlDefinition);

        return statefulContainer;
    }

    public abstract TState CreateState();
}
