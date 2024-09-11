using PixiEditor.SVG.Helpers;

namespace PixiEditor.SVG.Units;

public struct SvgEnumUnit<T> : ISvgUnit where T : Enum
{
    public T Value { get; set; }

    public SvgEnumUnit(T value)
    {
        Value = value;
    }

    public string ToXml()
    {
        return Value.ToString().ToKebabCase();
    }
}
