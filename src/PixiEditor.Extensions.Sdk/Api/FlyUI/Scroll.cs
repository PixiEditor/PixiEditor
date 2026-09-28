using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.Sdk.Attributes;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

[ControlTypeId("Scroll")]
public class Scroll : SingleChildLayoutElement
{
    public ScrollDirection Direction { get; set; } = ScrollDirection.Vertical;

    public Scroll(LayoutElement child = null, ScrollDirection direction = ScrollDirection.Vertical, Cursor? cursor = null) : base(cursor)
    {
        Direction = direction;
        Child = child;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition scroll = new ControlDefinition(UniqueId, GetType());

        scroll.AddProperty(Direction);

        if (Child != null)
        {
            scroll.AddChild(Child.BuildNative());
        }

        return scroll;
    }
}
