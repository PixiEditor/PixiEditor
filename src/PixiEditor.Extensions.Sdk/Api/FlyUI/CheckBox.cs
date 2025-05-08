using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class CheckBox : SingleChildLayoutElement
{
    public event ElementEventHandler CheckedChanged
    {
        add => AddEvent(nameof(CheckedChanged), value);
        remove => RemoveEvent(nameof(CheckedChanged), value);
    }

    public bool IsChecked { get; set; }

    public CheckBox(ILayoutElement<ControlDefinition> child = null, ElementEventHandler onCheckedChanged = null)
    {
        Child = child;
        
        if (onCheckedChanged != null)
        {
            CheckedChanged += (args) =>
            {
                IsChecked = !IsChecked;
                onCheckedChanged(args);
            };
        }
        else
        {
            CheckedChanged += args => IsChecked = !IsChecked;
        }
    }


    public override ControlDefinition BuildNative()
    {
        ControlDefinition checkbox = new ControlDefinition(UniqueId, "CheckBox");
        if (Child != null)
            checkbox.AddChild(Child.BuildNative());

        BuildPendingEvents(checkbox);
        return checkbox;
    }
}
