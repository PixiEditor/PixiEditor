using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Center : SingleChildLayoutElement
{
    public Center(ILayoutElement<ControlDefinition> child)
    {
        Child = child;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition center = new ControlDefinition(UniqueId, "Center");

        if (Child != null)
            center.AddChild(Child.BuildNative());

        return center;
    }
}
