namespace PixiEditor.SVG.Units;

public class SvgList : SvgProperty
{
    public char Separator { get; set; }
    public SvgList(string svgName, char separator) : base(svgName)
    {
        Separator = separator;
    }
}

public class SvgList<T> : SvgList where T : ISvgUnit
{

    public SvgList(string svgName, char separator, params T[] units) : base(svgName, separator)
    {
        
    }
}
