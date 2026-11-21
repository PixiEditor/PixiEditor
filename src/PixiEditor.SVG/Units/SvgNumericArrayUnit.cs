using Drawie.Numerics;
using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Units;

public struct SvgNumericArrayUnit : ISvgUnit
{
    public SvgNumericUnit[] Values { get; set; }

    public SvgNumericArrayUnit(SvgNumericUnit[] values)
    {
        Values = values;
    }

    public SvgNumericArrayUnit(float[] values)
    {
        Values = new SvgNumericUnit[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            Values[i] = new SvgNumericUnit(values[i], "");
        }
    }

    public SvgNumericArrayUnit(ColorMatrix matrix) : this(matrix.ToArray())
    {
    }

    public string ToXml(DefStorage defs)
    {
        return string.Join(",", Values.Select(v => v.ToXml(defs)));
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        if (string.IsNullOrEmpty(readerValue))
        {
            Values = Array.Empty<SvgNumericUnit>();
            return;
        }

        string[] commaSplitted =
            readerValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        List<string> values = new List<string>();
        foreach (var splitted in commaSplitted)
        {
            var spaceSplitted =
                splitted.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            values.AddRange(spaceSplitted);
        }

        List<SvgNumericUnit> parsedValues = new List<SvgNumericUnit>();
        foreach (var value in values)
        {
            var svgNumeric = new SvgNumericUnit();
            svgNumeric.ValuesFromXml(value, defs);
            parsedValues.Add(svgNumeric);
        }

        Values = parsedValues.ToArray();
    }
}
