using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("MultiplyMatrix")]
public class MultiplyMatrixNode : Node
{
    public FuncInputProperty<Float3x3> First { get; }
    public FuncInputProperty<Float3x3> Second { get; }
    public FuncOutputProperty<Float3x3> Result { get; }

    public MultiplyMatrixNode()
    {
        First = CreateFuncInput<Float3x3>("First", "FIRST_INPUT", new Float3x3("") { ConstantValue = Matrix3X3.Identity });
        Second = CreateFuncInput<Float3x3>("Second", "SECOND_INPUT", new Float3x3("") { ConstantValue = Matrix3X3.Identity });
        Result = CreateFuncOutput<Float3x3>("Result", "RESULT", CalculateMatrix);
    }

    protected Float3x3 CalculateMatrix(FuncContext ctx)
    {
        if (ctx.HasContext)
        {
            return ctx.NewFloat3x3(new Expression($"{ctx.GetValue(First).ExpressionValue} * {ctx.GetValue(Second).ExpressionValue}"));
        }

        var firstConst = ctx.GetValue(First).GetConstant() as Matrix3X3?;
        var secondConst = ctx.GetValue(Second).GetConstant() as Matrix3X3?;

        if (firstConst.HasValue && secondConst.HasValue)
        {
            return new Float3x3("") { ConstantValue = firstConst.Value.PostConcat(secondConst.Value) };
        }

        return new Float3x3("") { ConstantValue = Matrix3X3.Identity };
    }

    protected override void OnExecute(RenderContext context)
    {

    }

    public override Node CreateCopy()
    {
        return new MultiplyMatrixNode();
    }
}
