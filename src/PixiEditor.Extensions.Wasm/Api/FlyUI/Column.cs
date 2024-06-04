namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public class Column : MultiChildLayoutElement
{
    public Column(params LayoutElement[] children)
    {
        Children = new List<LayoutElement>(children);
    }
    
    public override CompiledControl BuildNative()
    {
        CompiledControl control = new CompiledControl(UniqueId, "Column");
        control.Children.AddRange(Children.Where(x => x != null).Select(x => x.BuildNative()));

        return control;
    }
}
