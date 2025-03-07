using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Units;

public struct SvgStringUnit : ISvgUnit
{
    public SvgStringUnit(string value)
    {
        Value = value;
    }

    public string Value { get; set; }
    public string ToXml()
    {
        return Value;
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        Value = readerValue;
    }
}
