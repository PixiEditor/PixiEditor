using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public abstract class SvgProperty
{
    protected SvgProperty(string svgName)
    {
        SvgName = svgName;
    }

    public string SvgName { get; set; }
    public ISvgUnit Value { get; set; }
}

public class SvgProperty<T> : SvgProperty where T : ISvgUnit
{
    public new T Value
    {
        get => (T)base.Value;
        set => base.Value = value;
    }

    public SvgProperty(string svgName) : base(svgName)
    {
    }
}
