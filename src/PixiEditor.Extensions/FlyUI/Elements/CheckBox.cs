using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class CheckBox : SingleChildLayoutElement
{
    private Avalonia.Controls.CheckBox checkbox;    
    public event ElementEventHandler<ToggleEventArgs> CheckedChanged
    {
        add => AddEvent(nameof(CheckedChanged), value);
        remove => RemoveEvent(nameof(CheckedChanged), value);
    }

    protected override Control CreateNativeControl()
    {
        checkbox = new Avalonia.Controls.CheckBox();
        Binding binding =
            new Binding(nameof(Child)) { Source = this, Converter = LayoutElementToNativeControlConverter.Instance };
        checkbox.Bind(ContentControl.ContentProperty, binding);

        checkbox.IsCheckedChanged += (sender, args) => RaiseEvent(
            nameof(CheckedChanged),
            new ToggleEventArgs((sender as Avalonia.Controls.CheckBox).IsChecked.Value) { Sender = this });

        return checkbox;
    }

    protected override void AddChild(Control child)
    {
        checkbox.Content = child;
    }

    protected override void RemoveChild()
    {
        checkbox.Content = null;
    }
}
