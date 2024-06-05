using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public class Padding : SingleChildLayoutElement
{
    public Edges Edges { get; set; } = Edges.All(0);
    
    public Padding(LayoutElement child = null, Edges edges = default)
    {
        Edges = edges;
        Child = child;
    }
    
    public override CompiledControl BuildNative()
    {
        CompiledControl control = new CompiledControl(UniqueId, "Padding");
        control.Children.Add(Child.BuildNative());
        
        control.AddProperty(Edges);

        return control;
    }
}
