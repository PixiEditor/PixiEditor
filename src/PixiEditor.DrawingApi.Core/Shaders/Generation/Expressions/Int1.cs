namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class Int1(string name) : ShaderExpressionVariable<int>(name)
{
    public override string ConstantValueString => ConstantValue.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public override Expression? OverrideExpression { get; set; }

    public static implicit operator Int1(int value) => new Int1("") { ConstantValue = value };
    public static explicit operator int(Int1 value) => value.ConstantValue;
}
