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


        if (ctx.HasContext)
        {
            var translationMatrix = ctx.NewFloat3x3(
                one, zero, zero,
                zero, one, zero,
                translation.X, translation.Y, one
            );

            return ctx.NewFloat3x3(ShaderMath.PostConcat(input, translationMatrix));
        }

        Matrix3X3 contextlessTranslationMatrix = Matrix3X3.CreateTranslation(
            (float)(translation.X.GetConstant() as double? ?? 0.0),
            (float)(translation.Y.GetConstant() as double? ?? 0.0)
        );
        return new Float3x3("")
        {
            ConstantValue = input.ConstantValue.PostConcat(contextlessTranslationMatrix)
        };
    }

    public override Node CreateCopy()
    {
        return new OffsetNode();
    }
}
