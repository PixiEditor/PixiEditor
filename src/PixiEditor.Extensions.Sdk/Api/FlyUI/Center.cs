using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Center : SingleChildLayoutElement
{
    public Center(ILayoutElement<ControlDefinition> child, Cursor? cursor = null) : base(cursor)
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
