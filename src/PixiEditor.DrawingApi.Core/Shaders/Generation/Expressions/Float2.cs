using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class Float2(string name, VecD constant) : ShaderExpressionVariable<VecD>(name, constant)
{
    public Float2(string name) : this(name, VecD.Zero)
    {
    }

    public override string ConstantValueString
    {
        get
        {
            string x = ConstantValue.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = ConstantValue.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return $"float2({x}, {y})";
        }
    }
}
