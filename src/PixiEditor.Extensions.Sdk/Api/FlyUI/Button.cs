using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Button : SingleChildLayoutElement
{
    public event ElementEventHandler Click
    {
        add => AddEvent(nameof(Click), value);
        remove => RemoveEvent(nameof(Click), value);
    }

    public Button(ILayoutElement<ControlDefinition> child = null, ElementEventHandler onClick = null)
    {
        Child = child;
        if (onClick != null)
            Click += onClick;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition button = new ControlDefinition(UniqueId, "Button");
        if (Child != null)
            button.AddChild(Child.BuildNative());

        return button;
    }
}
