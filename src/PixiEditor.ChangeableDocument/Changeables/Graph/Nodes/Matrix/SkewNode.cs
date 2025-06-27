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

        if (ctx.HasContext)
        {
            var skewMatrix = ctx.NewFloat3x3(
                one, skew.Y, zero,
                skew.X, one, zero,
                zero, zero, one
            );

            return ctx.NewFloat3x3(ShaderMath.PostConcat(input, skewMatrix));
        }

        Matrix3X3 skewContextlessMatrix = Matrix3X3.CreateSkew(
            (float)(skew.X.GetConstant() as double? ?? 0.0),
            (float)(skew.Y.GetConstant() as double? ?? 0.0)
        );

        return new Float3x3("")
        {
            ConstantValue = input.ConstantValue.PostConcat(skewContextlessMatrix)
        };
    }

    public override Node CreateCopy()
    {
        return new SkewNode();
    }
}
