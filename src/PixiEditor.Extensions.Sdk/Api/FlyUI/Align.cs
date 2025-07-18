using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk.Attributes;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

[ControlTypeId("Align")]
public class Align : SingleChildLayoutElement
{
    public Alignment Alignment { get; set; }

    public Align(Alignment alignment = Alignment.TopLeft, LayoutElement child = null, Cursor? cursor = null) : base(cursor)
    {
        Child = child;
        Alignment = alignment;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition controlDefinition = new ControlDefinition(UniqueId, GetType());
        controlDefinition.AddProperty((int)Alignment);
        
        if (Child != null)
            controlDefinition.AddChild(Child.BuildNative());

        return controlDefinition;
    }
}
