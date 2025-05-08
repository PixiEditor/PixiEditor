using PixiEditor.Extensions.CommonApi.FlyUI;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public sealed class Layout : SingleChildLayoutElement
{
    public Layout(ILayoutElement<ControlDefinition> body = null)
    {
        Child = body;
    }

    public override ControlDefinition BuildNative()
    {
        ControlDefinition layout = new ControlDefinition(UniqueId, "Layout");

        if (Child != null)
            layout.AddChild(Child.BuildNative());

        BuildPendingEvents(layout);
        return layout;
    }

}
