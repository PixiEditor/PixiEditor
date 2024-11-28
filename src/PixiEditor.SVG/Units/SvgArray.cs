namespace PixiEditor.SVG.Units;

public class SvgArray<T> : SvgProperty where T : ISvgUnit
{
    public T[] Units { get; set; }

    public SvgArray(string svgName, params T[] units) : base(svgName)
    {
        Units = units;
    }
}
