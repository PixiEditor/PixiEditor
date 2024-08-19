using System.Linq.Expressions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders.Generation;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.Numerics;
using Expression = PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions.Expression;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Context;

public class FuncContext
{
    public static FuncContext NoContext { get; } = new();

    /// <summary>
    ///     Original position of the pixel in the image. This is the input argument of the main function.
    /// </summary>
    public Float2 OriginalPosition { get; private set; }
    
    /// <summary>
    ///     Modified position of the pixel. This should be used to sample the texture, unless you want to sample the texture at the original position only.
    /// </summary>
    public Float2 SamplePosition { get; private set; }
    public VecI Size { get; private set; }
    public bool HasContext { get; private set; }
    public RenderingContext RenderingContext { get; set; }

    public ShaderBuilder Builder { get; set; }

    public void ThrowOnMissingContext()
    {
        if (!HasContext)
        {
            throw new NoNodeFuncContextException();
        }
    }

    public FuncContext()
    {
    }

    public FuncContext(RenderingContext renderingContext, ShaderBuilder builder)
    {
        RenderingContext = renderingContext;
        Builder = builder;
        HasContext = true;
        OriginalPosition = new Float2("coords"); // input argument 'half4 main(float2 coords)'
        SamplePosition = Builder.ConstructFloat2(OriginalPosition.X, OriginalPosition.Y); 
    }

    public Half4 SampleTexture(Texture imageValue, Float2 pos)
    {
        TextureSampler texName = Builder.AddOrGetTexture(imageValue);
        return Builder.Sample(texName, pos);
    }

    public Float2 NewFloat2(Expression x, Expression y)
    {
        return Builder.ConstructFloat2(x, y);
    }

    public Float1 NewFloat1(Expression result)
    {
        return Builder.ConstructFloat1(result);
    }


    public Int2 NewInt2(Expression first, Expression second)
    {
        return Builder.ConstructInt2(first, second);
    }

    public Half4 NewHalf4(Expression r, Expression g, Expression b, Expression a)
    {
        return Builder.ConstructHalf4(r, g, b, a);
    }


    public Half4 NewHalf4(Expression assignment)
    {
        return Builder.AssignNewHalf4(assignment);
    }

    public Float1 GetValue(FuncInputProperty<Float1> getFrom)
    {
        if (getFrom.Connection == null || !IsFuncType(getFrom))
        {
            string uniformName = $"float_{Builder.GetUniqueNameNumber()}";
            Builder.AddUniform(uniformName, (float)getFrom.Value(this).ConstantValue);
            return new Float1(uniformName);
        }

        return getFrom.Value(this);
    }

    public Expression GetValue(FuncInputProperty<Int1> getFrom)
    {
        if (getFrom.Connection == null || !IsFuncType(getFrom))
        {
            string uniformName = $"int_{Builder.GetUniqueNameNumber()}";
            Builder.AddUniform(uniformName, (int)getFrom.Value(this).ConstantValue);
            return new Expression(uniformName);
        }

        return getFrom.Value(this);
    }

    private static bool IsFuncType<T>(FuncInputProperty<T> getFrom)
    {
        return getFrom.Connection.ValueType.IsAssignableTo(typeof(Delegate));
    }

    public ShaderExpressionVariable GetValue(FuncInputProperty<Half4> getFrom)
    {
        if (getFrom.Connection == null || !IsFuncType(getFrom))
        {
            Half4 color = getFrom.Value(this);
            color.VariableName = $"color_{Builder.GetUniqueNameNumber()}";
            Builder.AddUniform(color.VariableName, color.ConstantValue);
            return color;
        }

        return getFrom.Value(this);
    }

    public Float2 GetValue(FuncInputProperty<Float2> getFrom)
    {
        if (getFrom.Connection == null || !IsFuncType(getFrom))
        {
            Float2 value = getFrom.Value(this);
            value.VariableName = $"float2_{Builder.GetUniqueNameNumber()}";
            Builder.AddUniform(value.VariableName, value.ConstantValue);
            return value;
        }

        return getFrom.Value(this);
    }
}
