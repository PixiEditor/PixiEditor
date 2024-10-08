﻿using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public abstract class MultiChildLayoutElement : LayoutElement, IMultiChildLayoutElement<CompiledControl>
{
    List<ILayoutElement<CompiledControl>> IMultiChildLayoutElement<CompiledControl>.Children
    {
        get => Children.Cast<ILayoutElement<CompiledControl>>().ToList();
        set => Children = value.Cast<LayoutElement>().ToList();
    }

    public List<LayoutElement> Children { get; set; }

    public abstract override CompiledControl BuildNative();

}
