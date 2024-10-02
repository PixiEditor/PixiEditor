using System;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public class Half3(string name) : ShaderExpressionVariable<VecD3>(name), IMultiValueVariable
{
    private Expression? _overrideExpression;
    public override string ConstantValueString => $"half3({ConstantValue.X}, {ConstantValue.Y}, {ConstantValue.Z})";
    
    public Float1 R => new Half3Float1Accessor(this, 'r') { ConstantValue = ConstantValue.X, OverrideExpression = _overrideExpression};
    public Float1 G => new Half3Float1Accessor(this, 'g') { ConstantValue = ConstantValue.X, OverrideExpression = _overrideExpression};
    public Float1 B => new Half3Float1Accessor(this, 'b') { ConstantValue = ConstantValue.Z, OverrideExpression = _overrideExpression};

    public override Expression? OverrideExpression
    {
        get => _overrideExpression;
        set
        {
            _overrideExpression = value;
        }
    }

    public ShaderExpressionVariable GetValueAt(int index)
    {
        return index switch
        {
            0 => R,
            1 => G,
            2 => B,
            _ => throw new IndexOutOfRangeException()
        };
    }

    public static string ConstructorText(Expression r, Expression g, Expression b) =>
        $"half3({r.ExpressionValue}, {g.ExpressionValue}, {b.ExpressionValue})";

    public static Expression Constructor(Expression r, Expression g, Expression b) => new(ConstructorText(r, g, b));
}
