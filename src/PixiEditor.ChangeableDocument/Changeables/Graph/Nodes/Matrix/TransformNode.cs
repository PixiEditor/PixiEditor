using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("Transform")]
public class TransformNode : Matrix3X3BaseNode
{
    public FuncInputProperty<Float2> Position { get; }
    public FuncOutputProperty<Float2> TransformedPosition { get; }

    public TransformNode()
    {
        Position = CreateFuncInput<Float2>("Position", "POSITION", VecD.Zero);
        TransformedPosition =
            CreateFuncOutput<Float2>("TransformedPosition", "TRANSFORMED_POSITION", TransformPosition);
    }

    private Float2 TransformPosition(FuncContext arg)
    {
        if (arg.HasContext)
        {
            Float3x3 matrix = CalculateMatrix(arg, arg.GetValue(Input));
            Float2 position = arg.GetValue(Position);
            Float3 toTransform = arg.Builder.ConstructFloat3(position.X, position.Y, new Float1("") { ConstantValue = 1 });
            Float3 transformed = arg.Builder.AssignNewFloat3(new Expression($"{matrix.ExpressionValue} * {toTransform.ExpressionValue}"));
            return arg.Builder.AssignNewFloat2(new Expression($"{transformed.ExpressionValue}.xy / {transformed.ExpressionValue}.z"));
        }

        return null;
    }

    public override Node CreateCopy()
    {
        return new TransformNode();
    }

    protected override Float3x3 CalculateMatrix(FuncContext ctx, Float3x3 input)
    {
        return input;
    }
}
