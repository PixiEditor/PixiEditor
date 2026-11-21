using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Units;

public struct SvgBoolUnit : ISvgUnit
{
    public bool Value { get; set; }

    public SvgBoolUnit(bool value)
    {
        Value = value;
    }

    public string ToXml(DefStorage defs)
    {
        return Value ? "true" : "false";
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        if(string.Equals(readerValue, "true", StringComparison.OrdinalIgnoreCase))
        {
            Value = true;
        }
        else if(string.Equals(readerValue, "false", StringComparison.OrdinalIgnoreCase))
        {
            Value = false;
        }
        else
        {
            throw new FormatException($"Invalid boolean value: {readerValue}");
        }
    }
}
