using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Padding : SingleChildLayoutElement
{
    public Edges Edges { get; set; } = Edges.All(0);
    
    public Padding(LayoutElement child = null, Edges edges = default)
    {
        Edges = edges;
        Child = child;
    }
    
    public override ControlDefinition BuildNative()
    {
        ControlDefinition controlDefinition = new ControlDefinition(UniqueId, "Padding");
        controlDefinition.Children.Add(Child.BuildNative());
        
        controlDefinition.AddProperty(Edges);

        return controlDefinition;
    }
}
