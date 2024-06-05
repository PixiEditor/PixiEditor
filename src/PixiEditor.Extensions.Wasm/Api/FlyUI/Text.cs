using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public class Text : StatelessElement
{
    public string Value { get; set; }
    
    public TextWrap TextWrap { get; set; }
    
    public FontStyle FontStyle { get; set; }
    
    public double FontSize { get; set; }
    
    public Text(string value, TextWrap wrap = TextWrap.None, FontStyle fontStyle = FontStyle.Normal, double fontSize = 12)
    {
        Value = value;
        TextWrap = wrap;
        FontStyle = fontStyle;
        FontSize = fontSize;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl text = new CompiledControl(UniqueId, "Text");
        text.AddStringProperty(Value);
        text.AddProperty((int)TextWrap);
        text.AddProperty((int)FontStyle);
        text.AddProperty(FontSize);

        BuildPendingEvents(text);
        return text;
    }
}
