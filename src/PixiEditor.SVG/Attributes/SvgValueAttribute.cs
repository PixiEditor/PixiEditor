namespace PixiEditor.SVG.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class SvgValueAttribute : Attribute
{
    public string Value { get; }

    public SvgValueAttribute(string value)
    {
        Value = value;
    }
}
