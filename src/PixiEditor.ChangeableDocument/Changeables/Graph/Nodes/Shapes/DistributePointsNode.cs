using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Numerics;
using ShapeData = PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data.ShapeData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("DistributePoints")]
public class DistributePointsNode : ShapeNode<PointsData>
{
    public InputProperty<int> MaxPointCount { get; }

    public InputProperty<int> Seed { get; }

    public DistributePointsNode()
    {
        MaxPointCount = CreateInput("MaxPointCount", "MAX_POINTS", 10).
            WithRules(v => v.Min(1));
        Seed = CreateInput("Seed", "SEED", 0);
    }

    protected override PointsData? GetShapeData(RenderingContext context)
    {
        return GetPointsRandomly(context.DocumentSize);
    }

    private PointsData GetPointsRandomly(VecI size)
    {
        var seed = Seed.Value;
        var random = new Random(seed);
        var pointCount = MaxPointCount.Value;

        List<VecD> finalPoints = new List<VecD>(pointCount);
        for (int i = 0; i < pointCount; i++)
        {
            finalPoints.Add(new VecD(random.NextDouble() * size.X, random.NextDouble() * size.Y));
        }
        
        var shapeData = new PointsData(finalPoints);
        return shapeData;
    }

    public override Node CreateCopy() => new DistributePointsNode();
}
