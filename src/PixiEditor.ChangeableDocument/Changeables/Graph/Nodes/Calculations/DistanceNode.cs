using Drawie.Backend.Core.Shaders.Generation.Expressions;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Calculations;

[NodeInfo("Distance")]
public class DistanceNode : Node
{
    public FuncInputProperty<Float2> Point1 { get; }
    public FuncInputProperty<Float2> Point2 { get; }
    public FuncOutputProperty<Float1> Distance { get; }

    public DistanceNode()
    {
        Point1 = CreateFuncInput(nameof(Point1), "POINT_1", new Float2(""));
        Point2 = CreateFuncInput(nameof(Point2), "POINT_2", new Float2(""));
        Distance = CreateFuncOutput<Float1>(nameof(Distance), "DISTANCE", Calculate);
    }

    protected override void OnExecute(RenderContext context)
    {

    }

    private Float1 Calculate(FuncContext context)
    {
        var point1 = context.GetValue(Point1);
        var point2 = context.GetValue(Point2);

        if (context.HasContext)
        {
            var result = ShaderMath.Distance(point1, point2);

            return context.NewFloat1(result);
        }

        var p1Const = (VecD)point1.GetConstant();
        var p2Const = (VecD)point2.GetConstant();

        var constValue = Math.Sqrt(Math.Pow(p2Const.X - p1Const.X, 2) + Math.Pow(p2Const.Y - p1Const.Y, 2));

        return new Float1(string.Empty) { ConstantValue = constValue };
    }

    public override Node CreateCopy()
    {
        return new DistanceNode();
    }
}
