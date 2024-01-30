using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Text : StatelessElement, IPropertyDeserializable
{
    private string _value = null!;
    public string Value { get => _value; set => SetField(ref _value, value); }
    public Text(string value = "")
    {
        Value = value;
    }

    public override Control BuildNative()
    {
        TextBlock textBlock = new();
        Binding binding = new()
        {
            Source = this,
            Path = nameof(Value),
        };

        textBlock.Bind(TextBlock.TextProperty, binding);
        return textBlock;
    }

    IEnumerable<object> IPropertyDeserializable.GetProperties()
    {
        yield return Value;
    }

    void IPropertyDeserializable.DeserializeProperties(IEnumerable<object> values)
    {
        Value = (string)values.ElementAt(0);
    }
}
