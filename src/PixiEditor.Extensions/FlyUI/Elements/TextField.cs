using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.FlyUI.Elements;

internal class TextField : LayoutElement
{
    private string text;
    private string? placeholder;
    public event ElementEventHandler TextChanged
    {
        add => AddEvent(nameof(TextChanged), value);
        remove => RemoveEvent(nameof(TextChanged), value);
    }

    public string Text { get => text; set => SetField(ref text, value); }

    public string? Placeholder { get => placeholder; set => SetField(ref placeholder, value); }

    public TextField(string text, string? placeholder = null)
    {
        Text = text;
        Placeholder = placeholder ?? string.Empty;
    }

    protected override Control CreateNativeControl()
    {
        TextBox textBox = new TextBox();

        Binding binding =
            new Binding(nameof(Text)) { Source = this, Mode = BindingMode.TwoWay };
        textBox.Bind(TextBox.TextProperty, binding);

        Binding placeholderBinding =
            new Binding(nameof(Placeholder)) { Source = this, Mode = BindingMode.OneWay };

        textBox.Bind(TextBox.WatermarkProperty, placeholderBinding);

        textBox.Padding = new Thickness(5);

        textBox.TextChanged += (s, e) =>
        {
            RaiseEvent(nameof(TextChanged), new TextEventArgs(textBox.Text));
        };

        return textBox;
    }

    protected override IEnumerable<object> GetControlProperties()
    {
        yield return Text;
        yield return Placeholder;
    }

    protected override void DeserializeControlProperties(List<object> values)
    {
        Text = (string)values[0];
        Placeholder = values.ElementAtOrDefault(1) as string;
    }
}
