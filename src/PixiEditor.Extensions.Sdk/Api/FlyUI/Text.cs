using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Text : StatelessElement
{
    public string Value { get; set; }
    
    public TextWrap TextWrap { get; set; }

    public TextStyle TextStyle { get; set; }
    
    public Text(string value, TextWrap wrap = TextWrap.None, TextStyle? textStyle = null)
    {
        Value = value;
        TextWrap = wrap;
        TextStyle = textStyle ?? TextStyle.Default;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition text = new ControlDefinition(UniqueId, "Text");
        text.AddProperty(Value);
        text.AddProperty(TextWrap);
        text.AddProperty(TextStyle);

        return text;
    }
}
