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

    public Float1 X
    {
        get
        {
            return new Float1($"{UniformName}.x", ConstantValue.X);
        }
    }
    
    public Float1 Y
    {
        get
        {
            return new Float1($"{UniformName}.y", ConstantValue.Y);
        }
    }
    
    public static implicit operator Float2(VecD value) => new Float2("", value);
    public static explicit operator VecD(Float2 value) => value.ConstantValue;
}
