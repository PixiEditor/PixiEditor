using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Align : SingleChildLayoutElement
{
    public Alignment Alignment { get; set; }

    public Align(Alignment alignment = Alignment.TopLeft, LayoutElement child = null)
    {
        Child = child;
        Alignment = alignment;
    }

    public override ControlDefinition BuildNative()
    {
        ControlDefinition controlDefinition = new ControlDefinition(UniqueId, "Align");
        controlDefinition.AddProperty((int)Alignment);
        
        if (Child != null)
            controlDefinition.AddChild(Child.BuildNative());

        BuildPendingEvents(controlDefinition);
        return controlDefinition;
    }
}
