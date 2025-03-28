using System.Xml.Linq;
using PixiEditor.SVG.Units;

namespace PixiEditor.SVG;

public abstract class SvgProperty
{
    protected SvgProperty(string svgName)
    {
        SvgName = svgName;
    }

    protected SvgProperty(string svgName, string? namespaceName) : this(svgName)
    {
        NamespaceName = namespaceName;
    }

    public string? NamespaceName { get; set; }
    public string SvgName { get; set; }
    public ISvgUnit? Unit { get; set; }
    public string? SvgFullName => NamespaceName == null ? SvgName : $"{NamespaceName}:{SvgName}";

    public ISvgUnit? CreateDefaultUnit()
    {
        var genericType = this.GetType().GetGenericArguments();
        if (genericType.Length == 0)
        {
            return null;
        }

        ISvgUnit unit = Activator.CreateInstance(genericType[0]) as ISvgUnit;
        if (unit == null)
        {
            throw new InvalidOperationException("Could not create unit");
        }

        return unit;
    }
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

    public SvgProperty(string svgName, string? namespaceName) : base(svgName, namespaceName)
    {
    }
}
