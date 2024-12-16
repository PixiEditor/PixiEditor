using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public abstract class SvgProperty
{
    protected SvgProperty(string svgName)
    {
        SvgName = svgName;
    }

    public string SvgName { get; set; }
    public ISvgUnit? Unit { get; set; }
}

public class SvgProperty<T> : SvgProperty where T : struct, ISvgUnit
{
    public new T? Unit
    {
        get => (T?)base.Unit;
        set => base.Unit = value;
    }
    
    public SvgProperty(string svgName) : base(svgName)
    {
    }
}
