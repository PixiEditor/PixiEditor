using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.Extensions.Sdk.Attributes;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

[ControlTypeId("TextField")]
public class TextField : LayoutElement
{
    public event ElementEventHandler<TextEventArgs> TextChanged
    {
        add => AddEvent(nameof(TextChanged), value);
        remove => RemoveEvent(nameof(TextChanged), value);
    }
    public string Text { get; set; }

    public string? Placeholder { get; set; } = string.Empty;

    public TextField(string? text = null, string? placeholder = null, Cursor? cursor = null) : base(cursor)
    {
        Text = text ?? string.Empty;
        Placeholder = placeholder;
        TextChanged += e => Text = e.Text;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition textField = new ControlDefinition(UniqueId, GetType());
        textField.AddProperty(Text);
        textField.AddProperty(Placeholder);
        return textField;
    }
}
