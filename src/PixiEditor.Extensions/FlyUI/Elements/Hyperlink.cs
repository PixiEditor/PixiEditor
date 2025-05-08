using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Hyperlink : Text
{
    public string Url { get; set; }

    public Hyperlink(string text, string url, TextWrap textWrap = TextWrap.None, TextStyle textStyle = default) : base(text, textWrap, textStyle)
    {
        Url = url;
    }

    public override Control BuildNative()
    {
        TextBlock hyperlink = (TextBlock)base.BuildNative();

        Binding urlBinding = new Binding() { Source = this, Path = nameof(Url), };

        hyperlink.Bind(UI.Hyperlink.UrlProperty, urlBinding);

        return hyperlink;
    }

    public override IEnumerable<object> GetProperties()
    {
        yield return Value;
        yield return TextWrap;
        yield return TextStyle;
        yield return Url;
    }

    public override void DeserializeProperties(ImmutableList<object> values)
    {
        base.DeserializeProperties(values);
        Url = (string)values[3];
    }
}
