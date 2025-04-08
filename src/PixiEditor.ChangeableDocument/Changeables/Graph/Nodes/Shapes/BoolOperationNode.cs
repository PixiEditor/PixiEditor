using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("BoolOperation")]
public class BoolOperationNode : Node
{
    public InputProperty<ShapeVectorData> ShapeA { get; }
    public InputProperty<ShapeVectorData> ShapeB { get; }
    public InputProperty<VectorPathOp> Operation { get; }
    public OutputProperty<ShapeVectorData> Result { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;

    public BoolOperationNode()
    {
        ShapeA = CreateInput<ShapeVectorData>("ShapeA", "FIRST_SHAPE", null);
        ShapeB = CreateInput<ShapeVectorData>("ShapeB", "SECOND_SHAPE", null);
        Operation = CreateInput<VectorPathOp>("Operation", "OPERATION", VectorPathOp.Union);
        Result = CreateOutput<ShapeVectorData>("Result", "RESULT", null);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (ShapeA.Value == null && ShapeB.Value == null)
        {
            Result.Value = null;
            return;
        }

        if (ShapeA.Value == null)
        {
            Result.Value = ShapeB.Value;
            return;
        }

        if (ShapeB.Value == null)
        {
            Result.Value = ShapeA.Value;
            return;
        }

        ShapeVectorData shapeA = ShapeA.Value;
        ShapeVectorData shapeB = ShapeB.Value;

        StrokeCap cap = StrokeCap.Round;
        StrokeJoin join = StrokeJoin.Round;
        PathFillType fillType = PathFillType.Winding;

        if (shapeA is PathVectorData pathA)
        {
            cap = pathA.StrokeLineCap;
            join = pathA.StrokeLineJoin;
        }
        else if (shapeB is PathVectorData pathB)
        {
            cap = pathB.StrokeLineCap;
            join = pathB.StrokeLineJoin;
        }

        Result.Value = new PathVectorData(shapeA.ToPath(true).Op(shapeB.ToPath(true), Operation.Value))
        {
            Fill = shapeA.Fill,
            Stroke = shapeA.Stroke,
            StrokeWidth = shapeA.StrokeWidth,
            FillPaintable = shapeA.FillPaintable,
            StrokeLineCap = cap,
            StrokeLineJoin = join,
        };
    }

    public override Node CreateCopy()
    {
        return new BoolOperationNode();
    }
}
