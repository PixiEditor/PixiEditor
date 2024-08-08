using System.Linq.Expressions;

namespace PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;

public static class ShaderMath
{
    public static Expression Add(Expression a, Expression b)
    {
        return new Expression($"{a.ExpressionValue} + {b.ExpressionValue}");
    }
    
    public static Expression Subtract(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"{a.VarOrConst()} - {b.VarOrConst()}");
    }
    
    public static Expression Multiply(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"{a.VarOrConst()} * {b.VarOrConst()}");
    }
    
    public static Expression Divide(ShaderExpressionVariable a, ShaderExpressionVariable b)
    {
        return new Expression($"{a.VarOrConst()} / {b.VarOrConst()}");
    }

    public static Expression Clamp(Expression value, Expression min, Expression max)
    {
        return new Expression($"clamp({value.ExpressionValue}, {min.ExpressionValue}, {max.ExpressionValue})");
    }

    public static Expression Sin(Expression x)
    {
        return new Expression($"sin({x.ExpressionValue})");
    }
    
    public static Expression Cos(Expression x)
    {
        return new Expression($"cos({x.ExpressionValue})");
    }
    
    public static Expression Tan(Expression x)
    {
        return new Expression($"tan({x.ExpressionValue})");
    }
}
