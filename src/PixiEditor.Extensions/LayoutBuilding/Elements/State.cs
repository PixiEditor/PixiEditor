using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.State;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public abstract class State : IState<Control>
{
    public abstract ILayoutElement<Control> Build();

    public void SetState(Action setAction)
    {
        setAction();
        StateChanged?.Invoke();
    }

    public event Action StateChanged;
}
