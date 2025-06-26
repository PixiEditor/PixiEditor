using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Rotate")]
public class RotateNode : Matrix3X3BaseNode
{
    public InputProperty<RotationType> RotationType { get; }
    public FuncInputProperty<Float1> Angle { get; }
    public FuncInputProperty<Float2> Center { get; }

    public RotateNode()
    {
        RotationType = CreateInput("RotationType", "UNIT", Nodes.Matrix.RotationType.Degrees);
        Angle = CreateFuncInput<Float1>("Angle", "ANGLE", 0.0);
        Center = CreateFuncInput<Float2>("Center", "CENTER", new VecD(0, 0));
    }

    protected override Float3x3 CalculateMatrix(FuncContext ctx, Float3x3 input)
    {
        Float1 angle = ctx.GetValue(Angle);

        Float2 center = ctx.GetValue(Center);

        Float1 one = new Float1("") { ConstantValue = 1.0 };
        Float1 zero = new Float1("") { ConstantValue = 0.0 };

        if (ctx.HasContext)
        {
            if (RotationType.Value == Nodes.Matrix.RotationType.Degrees)
            {
                angle = ctx.NewFloat1(ShaderMath.DegreesToRadians(angle));
            }

            var rotationMatrix = ctx.NewFloat3x3(
                ShaderMath.Cos(angle), ShaderMath.Sin(angle), zero,
                new Expression($"-{ShaderMath.Sin(angle).ExpressionValue}"), ShaderMath.Cos(angle), zero,
                new Expression(
                    $"{center.X.ExpressionValue} * (1.0 - {ShaderMath.Cos(angle)}) + {center.Y.ExpressionValue} * {ShaderMath.Sin(angle)}"), // m02 → col 2, row 0
                new Expression(
                    $"{center.Y.ExpressionValue} * (1.0 - {ShaderMath.Cos(angle)}) - {center.X.ExpressionValue} * {ShaderMath.Sin(angle)}"), // m12 → col 2, row 1
                one
            );

            return ctx.NewFloat3x3(ShaderMath.PostConcat(input, rotationMatrix));
        }

        Matrix3X3 rotationContextlessMatrix = RotationType.Value switch
        {
            Nodes.Matrix.RotationType.Degrees => Matrix3X3.CreateRotationDegrees(
                (float)(angle.GetConstant() as double? ?? 0.0), (float)(center.X.GetConstant() as double? ?? 0),
                (float)(center.Y.GetConstant() as double? ?? 0)),
            Nodes.Matrix.RotationType.Radians => Matrix3X3.CreateRotation(
                (float)(angle.GetConstant() as double? ?? 0.0), (float)(center.X.GetConstant() as double? ?? 0),
                (float)(center.Y.GetConstant() as double? ?? 0)),
            _ => throw new ArgumentOutOfRangeException()
        };

        return new Float3x3("") { ConstantValue = input.ConstantValue.PostConcat(rotationContextlessMatrix) };
    }

    public override Node CreateCopy()
    {
        return new RotateNode();
    }
}

public enum RotationType
{
    Degrees,
    Radians
}
