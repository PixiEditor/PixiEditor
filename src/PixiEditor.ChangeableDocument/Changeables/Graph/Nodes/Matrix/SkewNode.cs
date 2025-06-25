using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Skew")]
public class SkewNode : Matrix3X3BaseNode
{
    public FuncInputProperty<Float2> Skew { get; }

    public SkewNode()
    {
        Skew = CreateFuncInput<Float2>("Skew", "SKEW", VecD.Zero);
    }

    protected override Float3x3 CalculateMatrix(FuncContext ctx, Float3x3 input)
    {
        Float2 skew = ctx.GetValue(Skew);

        Float1 one = new Float1("") { ConstantValue = 1.0 };
        Float1 zero = new Float1("") { ConstantValue = 0.0 };

        var skewMatrix = ctx.NewFloat3x3(
            one, skew.X, zero,
            skew.Y, one, zero,
            zero, zero, one
        );

        if (ctx.HasContext)
        {
            return ctx.NewFloat3x3(ShaderMath.PostConcat(input, skewMatrix));
        }

        return new Float3x3("")
        {
            ConstantValue = input.ConstantValue.PostConcat(skewMatrix.ConstantValue)
        };
    }

    public override Node CreateCopy()
    {
        return new SkewNode();
    }
}
