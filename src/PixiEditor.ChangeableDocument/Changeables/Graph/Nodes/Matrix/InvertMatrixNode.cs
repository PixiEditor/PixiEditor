using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Shaders.Generation.Expressions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Matrix;

[NodeInfo("InvertMatrix")]
public class InvertMatrixNode : Matrix3X3BaseNode
{
    protected override Float3x3 CalculateMatrix(FuncContext ctx, Float3x3 input)
    {
        if (ctx.HasContext)
        {
            return ctx.NewFloat3x3(new Expression($"inverse({input.ExpressionValue})"));
        }

        var constant = input.GetConstant() as Matrix3X3?;
        if (constant.HasValue)
        {
            return new Float3x3("") { ConstantValue = constant.Value.Invert() };
        }

        return new Float3x3("") { ConstantValue = Matrix3X3.Identity };
    }

    public override Node CreateCopy()
    {
        return new InvertMatrixNode();
    }
}
