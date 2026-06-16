using System.Globalization;
using Drawie.Numerics;
using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Units;

public struct SvgRectUnit(RectD rect) : ISvgUnit
{
    public RectD Value { get; set; } = rect;
    public string ToXml(DefStorage defs)
    {
        return $"{Value.X} {Value.Y} {Value.Width} {Value.Height}";
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        string[] values = readerValue.Split(' ');
        
        if (values.Length == 4)
        {
            double x, y, width, height;
            
            x = TryParseOrZero(values[0]);
            y = TryParseOrZero(values[1]);
            width = TryParseOrZero(values[2]);
            height = TryParseOrZero(values[3]);
            
            Value = new RectD(x, y, width, height);
        }
        
        double TryParseOrZero(string value)
        {
            return double.TryParse(value, CultureInfo.InvariantCulture, out double result) ? result : 0;
        }
    }
}
