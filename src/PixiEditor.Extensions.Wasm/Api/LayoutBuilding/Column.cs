﻿namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Column : MultiChildLayoutElement
{
    public override CompiledControl BuildNative()
    {
        CompiledControl control = new CompiledControl(UniqueId, "Column");
        control.Children.AddRange(Children.Where(x => x != null).Select(x => x.BuildNative()));

        return control;
    }
}