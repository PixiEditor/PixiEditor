using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class CheckBox : SingleChildLayoutElement
{
    private bool isChecked;
    private Avalonia.Controls.CheckBox checkbox;

    public event ElementEventHandler<ToggleEventArgs> CheckedChanged
    {
        add => AddEvent(nameof(CheckedChanged), value);
        remove => RemoveEvent(nameof(CheckedChanged), value);
    }

    public bool IsChecked { get => isChecked; set => SetField(ref isChecked, value); }
    protected override Control CreateNativeControl()
    {
        checkbox = new Avalonia.Controls.CheckBox();

        Binding binding =
            new Binding(nameof(Child)) { Source = this, Converter = LayoutElementToNativeControlConverter.Instance };
        checkbox.Bind(ContentControl.ContentProperty, binding);

        Binding isCheckedBinding =
            new Binding(nameof(IsChecked)) { Source = this, Mode = BindingMode.TwoWay };

        checkbox.Bind(Avalonia.Controls.CheckBox.IsCheckedProperty, isCheckedBinding);

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

    protected override IEnumerable<object> GetControlProperties()
    {
        yield return IsChecked;
    }

    protected override void DeserializeControlProperties(List<object> values)
    {
        IsChecked = (bool)values[0];
    }
}
