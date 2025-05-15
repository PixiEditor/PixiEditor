using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("DistributePoints")]
public class DistributePointsNode : ShapeNode<PointsVectorData>
{
    public InputProperty<int> MaxPointCount { get; }

    public InputProperty<int> Seed { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;

    public DistributePointsNode()
    {
        MaxPointCount = CreateInput("MaxPointCount", "MAX_POINTS", 10).
            WithRules(v => v.Min(1));
        Seed = CreateInput("Seed", "SEED", 0);
    }

    protected override PointsVectorData? GetShapeData(RenderContext context)
    {
        return GetPointsRandomly(context.RenderOutputSize);
    }

    private PointsVectorData GetPointsRandomly(VecI size)
    {
        var seed = Seed.Value;
        var random = new Random(seed);
        var pointCount = MaxPointCount.Value;

        List<VecD> finalPoints = new List<VecD>(pointCount);
        for (int i = 0; i < pointCount; i++)
        {
            finalPoints.Add(new VecD(random.NextDouble() * size.X, random.NextDouble() * size.Y));
        }
        
        var shapeData = new PointsVectorData(finalPoints);
        return shapeData;
    }

    public override Node CreateCopy() => new DistributePointsNode();
}
