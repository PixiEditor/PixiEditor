using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.Sdk.Attributes;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

[ControlTypeId("Hyperlink")]
public class Hyperlink : Text
{
    public string Url { get; set; }

    public Hyperlink(string url, string text, TextWrap textWrap = TextWrap.None, TextStyle? textStyle = null, Cursor? cursor = null) : base(text, textWrap, textStyle, cursor)
    {
        Url = url;
    }

    protected override ControlDefinition CreateControl()
    {
        ControlDefinition hyperlink = new ControlDefinition(UniqueId, GetType());
        hyperlink.AddProperty(Value);
        hyperlink.AddProperty(TextWrap);
        hyperlink.AddProperty(TextStyle);
        hyperlink.AddProperty(Url);

        return hyperlink;
    }
}
