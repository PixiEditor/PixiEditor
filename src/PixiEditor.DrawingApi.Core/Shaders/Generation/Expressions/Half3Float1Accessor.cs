namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

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
