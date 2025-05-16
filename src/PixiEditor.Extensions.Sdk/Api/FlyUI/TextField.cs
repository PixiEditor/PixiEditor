using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class TextField : LayoutElement
{
    public event ElementEventHandler<TextEventArgs> TextChanged
    {
        add => AddEvent(nameof(TextChanged), value);
        remove => RemoveEvent(nameof(TextChanged), value);
    }
    public string Text { get; set; }

    public TextField(string? text = null, Cursor? cursor = null) : base(cursor)
    {
        Text = text ?? string.Empty;
        TextChanged += e => Text = e.Text;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition textField = new ControlDefinition(UniqueId, "TextField");
        textField.AddProperty(Text);
        return textField;
    }
}
