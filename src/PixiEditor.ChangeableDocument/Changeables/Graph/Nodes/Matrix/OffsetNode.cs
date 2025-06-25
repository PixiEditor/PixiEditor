using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Offset")]
public class OffsetNode : Matrix3X3BaseNode
{
    public FuncInputProperty<Float2> Translation { get; }

    public OffsetNode()
    {
        Translation = CreateFuncInput<Float2>("Offset", "OFFSET", VecD.Zero);
    }

    protected override Float3x3 CalculateMatrix(FuncContext ctx, Float3x3 input)
    {
        Float2 translation = ctx.GetValue(Translation);

        Float1 one = new Float1("") { ConstantValue = 1.0 };
        Float1 zero = new Float1("") { ConstantValue = 0.0 };

        var translationMatrix = ctx.NewFloat3x3(
            one, zero, translation.X,
            zero, one, translation.Y,
            zero, zero, one
        );

        if (ctx.HasContext)
        {
            return ctx.NewFloat3x3(ShaderMath.PostConcat(input, translationMatrix));
        }

        return new Float3x3("")
        {
            ConstantValue = input.ConstantValue.PostConcat(translationMatrix.ConstantValue)
        };
    }

    public override Node CreateCopy()
    {
        return new OffsetNode();
    }
}
