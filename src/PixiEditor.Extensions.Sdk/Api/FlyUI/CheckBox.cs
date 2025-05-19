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

    public CheckBox(ILayoutElement<ControlDefinition> child = null, bool isChecked = false, ElementEventHandler onCheckedChanged = null, Cursor? cursor = null) : base(cursor)
    {
        Child = child;
        IsChecked = isChecked;
        
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


    protected override ControlDefinition CreateControl()
    {
        ControlDefinition checkbox = new ControlDefinition(UniqueId, "CheckBox");
        if (Child != null)
            checkbox.AddChild(Child.BuildNative());

        checkbox.AddProperty(IsChecked);

        return checkbox;
    }
}
