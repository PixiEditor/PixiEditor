using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public class Button : SingleChildLayoutElement
{
    public event ElementEventHandler Click
    {
        add => AddEvent(nameof(Click), value);
        remove => RemoveEvent(nameof(Click), value);
    }

    public Button(ILayoutElement<CompiledControl> child = null, ElementEventHandler onClick = null)
    {
        Child = child;
        if (onClick != null)
            Click += onClick;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl button = new CompiledControl(UniqueId, "Button");
        if (Child != null)
            button.AddChild(Child.BuildNative());

        BuildPendingEvents(button);
        return button;
    }
}
