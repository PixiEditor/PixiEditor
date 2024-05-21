namespace PixiEditor.Extensions.CommonApi.FlyUI.State;

public interface IStatefulElement<out TBuild> : ILayoutElement<TBuild>
{
    public IState<TBuild> State { get; }
}

public interface IStatefulElement<out TBuild, out TState> : IStatefulElement<TBuild> where TState : IState<TBuild>
{
    public TState CreateState();
}
