using Drawie.Numerics;
using PixiEditor.SVG.Elements;

namespace PixiEditor.SVG.Units;

public struct SvgVectorUnit : ISvgUnit
{
    public SvgNumericUnit X { get; set; }
    public SvgNumericUnit Y { get; set; }

    public SvgVectorUnit()
    {
        X = new SvgNumericUnit();
        Y = new SvgNumericUnit();
    }

    public SvgVectorUnit(VecD vector)
    {
        X = new SvgNumericUnit(vector.X, "");
        Y = new SvgNumericUnit(vector.Y, "");
    }

    public string ToXml(DefStorage defs)
    {
        string xValue = X.ToXml(defs);
        string yValue = Y.ToXml(defs);

        return $"{xValue},{yValue}";
    }

    public void ValuesFromXml(string readerValue, SvgDefs defs)
    {
        var x = new SvgNumericUnit();
        var y = new SvgNumericUnit();
        if (string.IsNullOrEmpty(readerValue))
        {
            X = x;
            Y = y;
            return;
        }

        string[] values =
            readerValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (values.Length == 1)
        {
            x.ValuesFromXml(values[0], defs);
            y.ValuesFromXml(values[0], defs);
            X = x;
            Y = y;
            return;
        }

        if (values.Length != 2)
        {
            throw new FormatException("Invalid vector format. Expected two values.");
        }

        x.ValuesFromXml(values[0], defs);
        y.ValuesFromXml(values[1], defs);
        X = x;
        Y = y;
    }
}
