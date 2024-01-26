using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Button : SingleChildLayoutElement
{
    public event ElementEventHandler Click
    {
        add => AddEvent(nameof(Click), value);
        remove => RemoveEvent(nameof(Click), value);
    }

    public Button(ILayoutElement<NativeControl> child = null, ElementEventHandler onClick = null)
    {
        Child = child;
        if (onClick != null)
            Click += onClick;
    }

    public override NativeControl Build()
    {
        NativeControl button = new NativeControl("Button");
        if (Child != null)
            button.AddChild(Child.Build());

        return button;
    }
}
