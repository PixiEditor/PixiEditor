using System.Xml;
using Drawie.Numerics;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG.Elements;

public class SvgPolygon() : SvgPrimitive("polygon")
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
        return SvgPolyline.GetPoints(RawPoints.Unit?.Value ?? string.Empty);
    }
}
