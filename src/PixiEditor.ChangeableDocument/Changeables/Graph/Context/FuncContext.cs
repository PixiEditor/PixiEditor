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

    public Float2 Position { get; private set; }
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
        Position = new Float2("coords"); // input argument 'half4 main(float2 coords)'
    }

    public Half4 SampleTexture(Texture? imageValue, Float2 pos)
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

    public Half4 NewHalf4(Expression r, Expression g, Expression b, Expression a)
    {
        return Builder.ConstructHalf4(r, g, b, a);
    }
}
