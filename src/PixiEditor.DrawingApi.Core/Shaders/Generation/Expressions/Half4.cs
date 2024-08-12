using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class Half4(string name) : ShaderExpressionVariable<Color>(name)
{
    public override string ConstantValueString => $"half4({ConstantValue.R}, {ConstantValue.G}, {ConstantValue.B}, {ConstantValue.A})";
    
    public Float1 R => new Float1(string.IsNullOrEmpty(VariableName) ? string.Empty : $"{VariableName}.r") { ConstantValue = ConstantValue.R };
    public Float1 G => new Float1(string.IsNullOrEmpty(VariableName) ? string.Empty : $"{VariableName}.g") { ConstantValue = ConstantValue.G };
    public Float1 B => new Float1(string.IsNullOrEmpty(VariableName) ? string.Empty : $"{VariableName}.b") { ConstantValue = ConstantValue.B };
    public Float1 A => new Float1(string.IsNullOrEmpty(VariableName) ? string.Empty : $"{VariableName}.a") { ConstantValue = ConstantValue.A };
    
    public static implicit operator Half4(Color value) => new Half4("") { ConstantValue = value };
    public static explicit operator Color(Half4 value) => value.ConstantValue;
}
