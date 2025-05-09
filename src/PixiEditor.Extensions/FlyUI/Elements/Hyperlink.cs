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

    protected override Control CreateNativeControl()
    {
        TextBlock hyperlink = (TextBlock)base.CreateNativeControl();

        Binding urlBinding = new Binding() { Source = this, Path = nameof(Url), };

        hyperlink.Bind(UI.Hyperlink.UrlProperty, urlBinding);

        return hyperlink;
    }

    protected override IEnumerable<object> GetControlProperties()
    {
        yield return Value;
        yield return TextWrap;
        yield return TextStyle;
        yield return Url;
    }

    protected override void DeserializeControlProperties(List<object> values)
    {
        base.DeserializeControlProperties(values);
        Url = (string)values[3];
    }
}
