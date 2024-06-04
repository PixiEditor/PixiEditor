using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using PixiEditor.Extensions.CommonApi.FlyUI.Properties;
using PixiEditor.Extensions.FlyUI.Converters;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Text : StatelessElement, IPropertyDeserializable
{
    private string _value = null!;
    public string Value { get => _value; set => SetField(ref _value, value); }
    
    public TextWrap TextWrap { get; set; } = TextWrap.None;

    public Text()
    {

    }

    public Text(string value = "", TextWrap textWrap = TextWrap.None)
    {
        Value = value;
        TextWrap = textWrap;
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

        textBlock.Bind(TextBlock.TextProperty, valueBinding);
        textBlock.Bind(TextBlock.TextWrappingProperty, textWrapBinding);
        return textBlock;
    }

    IEnumerable<object> IPropertyDeserializable.GetProperties()
    {
        yield return Value;
        yield return TextWrap;
    }

    void IPropertyDeserializable.DeserializeProperties(IEnumerable<object> values)
    {
        Value = (string)values.ElementAt(0);
        TextWrap = (TextWrap)values.ElementAt(1);
    }
}
