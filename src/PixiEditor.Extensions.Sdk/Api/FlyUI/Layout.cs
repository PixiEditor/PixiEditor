using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public sealed class Layout : SingleChildLayoutElement
{
    public Layout(ILayoutElement<ControlDefinition> body = null)
    {
        Child = body;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition layout = new ControlDefinition(UniqueId, "Layout");

        if (Child != null)
            layout.AddChild(Child.BuildNative());

        return layout;
    }

}
