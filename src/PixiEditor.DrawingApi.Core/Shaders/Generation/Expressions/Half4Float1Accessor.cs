namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

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
