using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Scale")]
public class ScaleNode : Matrix3X3BaseNode
{
    public FuncInputProperty<Float2> Scale { get; }
    public FuncInputProperty<Float2> Center { get; }

    public ScaleNode()
    {
        Scale = CreateFuncInput<Float2>("Scale", "SCALE", new VecD(1, 1));
        Center = CreateFuncInput<Float2>("Center", "CENTER", new VecD(0, 0));
    }

    protected override Float3x3 CalculateMatrix(FuncContext ctx, Float3x3 input)
    {
        Float2 scale = ctx.GetValue(Scale);
        Float2 center = ctx.GetValue(Center);

        Float1 one = new Float1("") { ConstantValue = 1.0 };
        Float1 zero = new Float1("") { ConstantValue = 0.0 };

        if (ctx.HasContext)
        {
            var scaleMatrix = ctx.NewFloat3x3(
                scale.X, zero, zero,
                zero, scale.Y, zero,
                new Expression($"{center.X.ExpressionValue} * (1.0 - {scale.X.ExpressionValue})"),
                new Expression($"{center.Y.ExpressionValue} * (1.0 - {scale.Y.ExpressionValue})"),
                one
            );

            return ctx.NewFloat3x3(ShaderMath.PostConcat(input, scaleMatrix));
        }

        Matrix3X3 scaleContextlessMatrix = Matrix3X3.CreateScale(
            (float)(scale.X.GetConstant() as double? ?? 1.0f),
            (float)(scale.Y.GetConstant() as double? ?? 1.0f),
            (float)(center.X.GetConstant() as double? ?? 0.0f),
            (float)(center.Y.GetConstant() as double? ?? 0.0f)
        );

        return new Float3x3("") { ConstantValue = input.ConstantValue.PostConcat(scaleContextlessMatrix) };
    }

    /*protected override Matrix3X3 CalculateMatrix(Matrix3X3 input)
    {
        Matrix3X3 scaled = Matrix3X3.CreateScale((float)Scale.Value.X, (float)Scale.Value.Y, (float)Center.Value.X, (float)Center.Value.Y);
        return input.PostConcat(scaled);
    }*/

    public override Node CreateCopy()
    {
        return new ScaleNode();
    }
}
