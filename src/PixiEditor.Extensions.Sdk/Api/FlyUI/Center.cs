using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Center : SingleChildLayoutElement
{
    public Center(ILayoutElement<ControlDefinition> child)
    {
        Child = child;
    }

    public override ControlDefinition BuildNative()
    {
        ControlDefinition center = new ControlDefinition(UniqueId, "Center");

        if (Child != null)
            center.AddChild(Child.BuildNative());

        BuildPendingEvents(center);
        return center;
    }
}
