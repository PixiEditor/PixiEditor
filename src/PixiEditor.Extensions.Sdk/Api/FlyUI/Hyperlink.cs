using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Hyperlink : Text
{
    public string Url { get; set; }

    public Hyperlink(string url, string text, TextWrap textWrap = TextWrap.None, TextStyle? textStyle = null) : base(text, textWrap, textStyle)
    {
        Url = url;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl hyperlink = new CompiledControl(UniqueId, "Hyperlink");
        hyperlink.AddProperty(Value);
        hyperlink.AddProperty(TextWrap);
        hyperlink.AddProperty(TextStyle);
        hyperlink.AddProperty(Url);

        BuildPendingEvents(hyperlink);
        return hyperlink;
    }
}
