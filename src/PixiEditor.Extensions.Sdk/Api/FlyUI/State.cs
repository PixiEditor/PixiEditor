﻿using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.State;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

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