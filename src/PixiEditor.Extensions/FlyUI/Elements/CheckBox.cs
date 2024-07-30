using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class CheckBox : SingleChildLayoutElement
{
    
    public event ElementEventHandler<ToggleEventArgs> CheckedChanged
    {
        add => AddEvent(nameof(CheckedChanged), value);
        remove => RemoveEvent(nameof(CheckedChanged), value);
    }

    public override Control BuildNative()
    {
        Avalonia.Controls.CheckBox checkbox = new Avalonia.Controls.CheckBox();
        Binding binding =
            new Binding(nameof(Child)) { Source = this, Converter = LayoutElementToNativeControlConverter.Instance };
        checkbox.Bind(ContentControl.ContentProperty, binding);

        checkbox.IsCheckedChanged += (sender, args) => RaiseEvent(
            nameof(CheckedChanged),
            new ToggleEventArgs((sender as Avalonia.Controls.CheckBox).IsChecked.Value) { Sender = this });

        return checkbox;
    }
}
