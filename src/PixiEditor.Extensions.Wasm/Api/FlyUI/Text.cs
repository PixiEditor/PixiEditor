using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Wasm.Api.FlyUI;

public class Text(string value, TextWrap wrap = TextWrap.None) : StatelessElement
{
    public string Value { get; set; } = value;
    
    public TextWrap TextWrap { get; set; } = wrap;

    public override CompiledControl BuildNative()
    {
        CompiledControl text = new CompiledControl(UniqueId, "Text");
        text.AddProperty(Value, typeof(string));
        text.AddProperty((int)TextWrap);

        BuildPendingEvents(text);
        return text;
    }
}
