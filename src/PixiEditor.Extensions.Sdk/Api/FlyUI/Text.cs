using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk.Attributes;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

[ControlTypeId("Text")]
public class Text : LayoutElement
{
    public string Value { get; set; }
    
    public TextWrap TextWrap { get; set; }

    public TextStyle TextStyle { get; set; }
    
    public Text(string value, TextWrap wrap = TextWrap.None, TextStyle? textStyle = null, Cursor? cursor = null) : base(cursor)
    {
        Value = value;
        TextWrap = wrap;
        TextStyle = textStyle ?? TextStyle.Default;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition text = new ControlDefinition(UniqueId, GetType());
        text.AddProperty(Value);
        text.AddProperty(TextWrap);
        text.AddProperty(TextStyle);

        return text;
    }
}
