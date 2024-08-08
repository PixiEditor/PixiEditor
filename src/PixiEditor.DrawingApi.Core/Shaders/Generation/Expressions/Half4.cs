using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class Half4(string name, Color constant) : ShaderExpressionVariable<Color>(name, constant)
{
    public Half4(string name) : this(name, Colors.Transparent)
    {
    }

    public override string ConstantValueString => $"half4({ConstantValue.R}, {ConstantValue.G}, {ConstantValue.B}, {ConstantValue.A})";
    
    public Float1 R => new Float1($"{UniformName}.r", ConstantValue.R);
    public Float1 G => new Float1($"{UniformName}.g", ConstantValue.G);
    public Float1 B => new Float1($"{UniformName}.b", ConstantValue.B);
    public Float1 A => new Float1($"{UniformName}.a", ConstantValue.A);
    
    public static implicit operator Half4(Color value) => new Half4("", value);
    public static explicit operator Color(Half4 value) => value.ConstantValue;
}
