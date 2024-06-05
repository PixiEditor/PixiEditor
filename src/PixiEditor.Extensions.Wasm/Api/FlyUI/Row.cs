namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public class Row : MultiChildLayoutElement
{
    public Row(params LayoutElement[] children)
    {
        Children = new List<LayoutElement>(children);
    }
    
    public override CompiledControl BuildNative()
    {
        CompiledControl control = new CompiledControl(UniqueId, "Row");
        control.Children.AddRange(Children.Select(x => x.BuildNative()));

        return control;
    }
}
