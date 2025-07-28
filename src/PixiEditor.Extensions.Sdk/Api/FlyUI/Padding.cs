using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk.Attributes;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

[ControlTypeId("Padding")]
public class Padding : SingleChildLayoutElement
{
    public Edges Edges { get; set; } = Edges.All(0);
    
    public Padding(LayoutElement child = null, Edges edges = default, Cursor? cursor = null) : base(cursor)
    {
        Edges = edges;
        Child = child;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition controlDefinition = new ControlDefinition(UniqueId, GetType());
        controlDefinition.Children.Add(Child.BuildNative());
        
        controlDefinition.AddProperty(Edges);

        return controlDefinition;
    }
}
