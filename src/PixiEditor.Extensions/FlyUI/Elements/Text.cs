using System.Collections.Immutable;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.FlyUI.Converters;
using FontStyle = PixiEditor.Extensions.CommonApi.FlyUI.Properties.FontStyle;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Text : StatelessElement, IPropertyDeserializable
{
    private TextWrap _textWrap = TextWrap.None;
    private string _value = null!;
    private FontStyle _fontStyle = FontStyle.Normal;
 
    public string Value { get => _value; set => SetField(ref _value, value); }
    public TextWrap TextWrap { get => _textWrap; set => SetField(ref _textWrap, value); }
    public FontStyle FontStyle { get => _fontStyle; set => SetField(ref _fontStyle, value); }

    public Text()
    {
    }

    public Text(string value = "", TextWrap textWrap = TextWrap.None, FontStyle fontStyle = FontStyle.Normal)
    {
        Value = value;
        TextWrap = textWrap;
        FontStyle = fontStyle;
    }

    public override Control BuildNative()
    {
        TextBlock textBlock = new();
        Binding valueBinding = new()
        {
            Source = this,
            Path = nameof(Value),
        };
        
        Binding textWrapBinding = new()
        {
            Source = this,
            Path = nameof(TextWrap),
            Converter = new EnumToEnumConverter<TextWrap, TextWrapping>(),
        };
        
        Binding fontStyleBinding = new()
        {
            Source = this,
            Path = nameof(FontStyle),
            Converter = new EnumToEnumConverter<FontStyle, Avalonia.Media.FontStyle>(),
        };
        
        textBlock.Bind(TextBlock.TextProperty, valueBinding);
        textBlock.Bind(TextBlock.TextWrappingProperty, textWrapBinding);
        textBlock.Bind(TextBlock.FontStyleProperty, fontStyleBinding); //TODO: Inter font doesn't work
        return textBlock;
    }

    IEnumerable<object> IPropertyDeserializable.GetProperties()
    {
        yield return Value;
        yield return TextWrap;
        yield return FontStyle;
    }

    void IPropertyDeserializable.DeserializeProperties(ImmutableList<object> values)
    {
        Value = (string)values.ElementAtOrDefault(0);
        TextWrap = (TextWrap)values.ElementAtOrDefault(1);
        FontStyle = (FontStyle)values.ElementAtOrDefault(2);
    }
}
