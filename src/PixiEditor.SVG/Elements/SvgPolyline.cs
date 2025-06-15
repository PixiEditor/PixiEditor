using System.Xml;
using Drawie.Numerics;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgPolyline() : SvgPrimitive("polyline")
{
    public SvgProperty<SvgStringUnit> RawPoints { get; } = new SvgProperty<SvgStringUnit>("points");
    public SvgProperty<SvgNumericUnit> PathLength { get; set; } = new SvgProperty<SvgNumericUnit>("pathLength");

    protected override IEnumerable<SvgProperty> GetProperties()
    {
        yield return RawPoints;
        yield return PathLength;
    }

    public VecD[] GetPoints()
    {
        return GetPoints(RawPoints.Unit?.Value ?? string.Empty);
    }

    public static VecD[] GetPoints(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        double? x = null;
        bool nextSpaceIsSeparator = false;
        string currentNumberString = string.Empty;

        List<VecD> points = new List<VecD>();
        foreach (char character in input)
        {
            if (char.IsWhiteSpace(character))
            {
                if (nextSpaceIsSeparator)
                {
                    if (x.HasValue)
                    {
                        points.Add(new VecD(x.Value, ParseNumber(currentNumberString)));
                        x = null;
                        currentNumberString = string.Empty;
                    }

                    nextSpaceIsSeparator = false;
                }
                else
                {
                    x = ParseNumber(currentNumberString);
                    currentNumberString = string.Empty;
                }
            }
            else if (char.IsDigit(character) || character == '.' || character == '-' || character == '+')
            {
                currentNumberString += character;
                nextSpaceIsSeparator = x.HasValue;
            }
            else if (character == ',')
            {
                x = ParseNumber(currentNumberString);
                currentNumberString = string.Empty;
                nextSpaceIsSeparator = false;
            }
        }

        if (currentNumberString.Length > 0)
        {
            if (x.HasValue)
            {
                points.Add(new VecD(x.Value, ParseNumber(currentNumberString)));
            }
            else
            {
                points.Add(new VecD(ParseNumber(currentNumberString), 0));
            }
        }

        return points.ToArray();
    }

    private static double ParseNumber(string currentNumberString)
    {
        return double.Parse(currentNumberString, System.Globalization.CultureInfo.InvariantCulture);
    }
}
