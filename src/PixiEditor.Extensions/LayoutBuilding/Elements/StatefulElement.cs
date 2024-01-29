using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.State;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public abstract class StatefulElement<TState> : LayoutElement, IStatefulElement<Control, TState> where TState : IState<Control>
{
    private TState? _state;
    private ContentPresenter _presenter = null!;

    IState<Control> IStatefulElement<Control>.State
    {
        get
        {
            if (_state == null)
            {
                _state = CreateState();
                _state.StateChanged += () => BuildNative();
            }

            return _state;
        }
    }

    public TState State => (TState)((IStatefulElement<Control>)this).State;

    public override Control BuildNative()
    {
        _presenter ??= new ContentPresenter();
        _presenter.Content = State.Build().BuildNative();
        return _presenter;
    }

    public abstract TState CreateState();
}
