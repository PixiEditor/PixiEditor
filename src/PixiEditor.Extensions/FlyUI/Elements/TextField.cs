using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.FlyUI.Elements;

internal class TextField : LayoutElement
{
    public event ElementEventHandler TextChanged
    {
        add => AddEvent(nameof(TextChanged), value);
        remove => RemoveEvent(nameof(TextChanged), value);
    }

    public string Text { get; set; }

    public TextField(string text)
    {
        Text = text;
    }

    protected override Control CreateNativeControl()
    {
        TextBox textBox = new TextBox();

        Binding binding =
            new Binding(nameof(Text)) { Source = this, Mode = BindingMode.TwoWay };
        textBox.Bind(TextBox.TextProperty, binding);

        textBox.TextChanged += (s, e) =>
        {
            RaiseEvent(nameof(TextChanged), new TextEventArgs(textBox.Text));
        };

        return textBox;
    }

    protected override void DeserializeControlProperties(List<object> values)
    {
        Text = (string)values[0];
    }
}
