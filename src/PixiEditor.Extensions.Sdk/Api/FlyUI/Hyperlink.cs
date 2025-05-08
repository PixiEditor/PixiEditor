using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Hyperlink : Text
{
    public string Url { get; set; }

    public Hyperlink(string url, string text, TextWrap textWrap = TextWrap.None, TextStyle? textStyle = null) : base(text, textWrap, textStyle)
    {
        Url = url;
    }

    public override ControlDefinition BuildNative()
    {
        ControlDefinition hyperlink = new ControlDefinition(UniqueId, "Hyperlink");
        hyperlink.AddProperty(Value);
        hyperlink.AddProperty(TextWrap);
        hyperlink.AddProperty(TextStyle);
        hyperlink.AddProperty(Url);

        BuildPendingEvents(hyperlink);
        return hyperlink;
    }
}
