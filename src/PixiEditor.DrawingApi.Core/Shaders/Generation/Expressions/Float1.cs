namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

/// <summary>
///     This is a shader type that represents a high precision floating point value. For medium precision see Short type.
/// </summary>
/// <param name="name">Name of the variable in shader code</param>
/// <param name="constant">Constant value of the variable.</param>
public class Float1(string name) : ShaderExpressionVariable<double>(name)
{
    public override string ConstantValueString =>
        ConstantValue.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public override Expression? OverrideExpression { get; set; }

    public static implicit operator Float1(double value) => new("") { ConstantValue = value };

    public static explicit operator double(Float1 value) => value.ConstantValue;

    public byte FullSizeByteConstant => (byte)(ConstantValue * 255);
}

public class Half4Float1Accessor : Float1
{
    public Half4Float1Accessor(Half4 accessTo, char name) : base(string.IsNullOrEmpty(accessTo.VariableName) ? string.Empty : $"{accessTo.VariableName}.{name}")
    {
        Accesses = accessTo;
    }
    
    public Half4 Accesses { get; }

    public static bool AllAccessSame(Expression r, Expression g, Expression b, Expression a, out Half4? half4)
    {
        if (r is Half4Float1Accessor rA && g is Half4Float1Accessor gA &&
            b is Half4Float1Accessor bA && a is Half4Float1Accessor aA &&
            rA.Accesses == gA.Accesses && bA.Accesses == aA.Accesses && rA.Accesses == bA.Accesses)
        {
            half4 = rA.Accesses;
            return true;
        }

        half4 = null;
        return false;
    }

    public static Expression GetOrConstructorExpressionHalf4(Expression r, Expression g, Expression b, Expression a) => AllAccessSame(r, g, b, a, out var value) ? value : Half4.Constructor(r, g, b, a);
}

public class Half3Float1Accessor : Float1
{
    public Half3Float1Accessor(Half3 accessTo, char name) : base(string.IsNullOrEmpty(accessTo.VariableName) ? string.Empty : $"{accessTo.VariableName}.{name}")
    {
        Accesses = accessTo;
    }
    
    public Half3 Accesses { get; }

    public static bool AllAccessSame(Expression r, Expression g, Expression b, out Half3? half3)
    {
        if (r is Half3Float1Accessor rA && g is Half3Float1Accessor gA && b is Half3Float1Accessor bA &&
            rA.Accesses == gA.Accesses && rA.Accesses == bA.Accesses)
        {
            half3 = rA.Accesses;
            return true;
        }

        half3 = null;
        return false;
    }
    
    public static Expression GetOrConstructorExpressionHalf3(Expression r, Expression g, Expression b) => AllAccessSame(r, g, b, out var value) ? value : Half3.Constructor(r, g, b);
}
