using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;

[NodeInfo("DistributePoints", "DISTRIBUTE_POINTS")]
public class DistributePointsNode : Node
{
    public OutputProperty<PointList> Points { get; }

    public InputProperty<int> MaxPointCount { get; }

    public InputProperty<int> Seed { get; }

    public DistributePointsNode()
    {
        Points = CreateOutput(nameof(Points), "POINTS", PointList.Empty);

        MaxPointCount = CreateInput("MaxPointCount", "MAX_POINTS", 10);
        Seed = CreateInput("Seed", "SEED", 0);
    }

    protected override Surface? OnExecute(RenderingContext context)
    {
        Points.Value = GetPointsRandomly();
        
        return null;
    }

    private PointList GetPointsRandomly()
    {
        var seed = Seed.Value;
        var random = new Random(seed);
        var pointCount = MaxPointCount.Value;
        var finalPoints = new PointList(pointCount)
        {
            HashValue = HashCode.Combine(pointCount, seed)
        };

        for (int i = 0; i < pointCount; i++)
        {
            finalPoints.Add(new VecD(random.NextDouble(), random.NextDouble()));
        }
        
        return finalPoints;
    }

    public override Node CreateCopy() => new DistributePointsNode();
}
