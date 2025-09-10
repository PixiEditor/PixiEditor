using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Transform")]
public class TransformNode : Matrix3X3BaseNode
{
    public FuncInputProperty<Float2, ShaderFuncContext> Position { get; }
    public FuncOutputProperty<Float2, ShaderFuncContext> TransformedPosition { get; }

    public TransformNode()
    {
        Position = CreateFuncInput<Float2, ShaderFuncContext>("Position", "POSITION", VecD.Zero);
        TransformedPosition =
            CreateFuncOutput<Float2, ShaderFuncContext>("TransformedPosition", "TRANSFORMED_POSITION", TransformPosition);
    }

    private Float2 TransformPosition(ShaderFuncContext arg)
    {
        if (arg.HasContext)
        {
            Float3x3 matrix = CalculateMatrix(arg, arg.GetValue(Input));
            Float2 position = arg.GetValue(Position);
            Float3 toTransform = arg.Builder.ConstructFloat3(position.X, position.Y, new Float1("") { ConstantValue = 1 });
            Float3 transformed = arg.Builder.AssignNewFloat3(new Expression($"{matrix.ExpressionValue} * {toTransform.ExpressionValue}"));
            return arg.Builder.AssignNewFloat2(new Expression($"{transformed.ExpressionValue}.xy / {transformed.ExpressionValue}.z"));
        }

        Float3x3 contextlessMatrix = CalculateMatrix(arg, arg.GetValue(Input));
        Float2 contextlessPosition = (arg.GetValue(Position).GetConstant() as VecD?) ?? VecD.Zero;
        return contextlessMatrix.ConstantValue.MapPoint(
            (float)contextlessPosition.X, (float)contextlessPosition.Y);
    }

    public override Node CreateCopy()
    {
        return new TransformNode();
    }

    protected override Float3x3 CalculateMatrix(ShaderFuncContext ctx, Float3x3 input)
    {
        return input;
    }
}
