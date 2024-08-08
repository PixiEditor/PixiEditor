using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class Float2(string name) : ShaderExpressionVariable<VecD>(name)
{
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
            return new Float1($"{UniformName}.x") { ConstantValue = ConstantValue.X };
        }
    }
    
    public Float1 Y
    {
        get
        {
            return new Float1($"{UniformName}.y") { ConstantValue = ConstantValue.Y };
        }
    }
    
    public static implicit operator Float2(VecD value) => new Float2("") { ConstantValue = value };
    public static explicit operator VecD(Float2 value) => value.ConstantValue;
}
