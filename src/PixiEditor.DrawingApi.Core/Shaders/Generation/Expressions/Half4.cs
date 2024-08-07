using PixiEditor.DrawingApi.Core.ColorsImpl;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class Half4(string name, Color constant) : ShaderExpressionVariable<Color>(name, constant)
{
    public Half4(string name) : this(name, Colors.Transparent)
    {
    }

    public override string ConstantValueString => $"half4({ConstantValue.R}, {ConstantValue.G}, {ConstantValue.B}, {ConstantValue.A})";
}
