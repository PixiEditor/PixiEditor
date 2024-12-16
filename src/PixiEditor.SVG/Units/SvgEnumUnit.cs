using PixiEditor.SVG.Helpers;

namespace PixiEditor.SVG.Units;

public struct SvgEnumUnit<T> : ISvgUnit where T : struct, Enum
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

    public void ValuesFromXml(string readerValue)
    {
        if (Enum.TryParse(readerValue.FromKebabToTitleCase(), out T result))
        {
            Value = result;
        }
    }
}
