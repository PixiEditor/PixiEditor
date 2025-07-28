using System.Diagnostics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("RemoveClosePoints")]
public class RemoveClosePointsNode : ShapeNode<PointsVectorData>
{
    public InputProperty<PointsVectorData> Input { get; }

    public InputProperty<double> MinDistance { get; }

    public InputProperty<int> Seed { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;

    public RemoveClosePointsNode()
    {
        Input = CreateInput<PointsVectorData>("Input", "POINTS", null);
        MinDistance = CreateInput("MinDistance", "MIN_DISTANCE", 0d);
        Seed = CreateInput("Seed", "SEED", 0);
    }

    protected override PointsVectorData? GetShapeData(RenderContext context)
    {
        var data = Input.Value;

        var distance = MinDistance.Value;
        var minDistanceSquared = distance * distance;

        if (distance == 0)
        {
            return data;
        }

        if (data?.Points == null)
        {
            return null;
        }

        var availablePoints = data.Points.ToList();
        List<VecD> newPoints = new List<VecD>();

        var random = new Random(Seed.Value);
        while (availablePoints.Count > 1)
        {
            var index = random.Next(availablePoints.Count);
            var point = availablePoints[index];

            newPoints.Add(point);
            availablePoints.RemoveAt(index);

            foreach (var remove in availablePoints.Where(InRange).ToList())
            {
                availablePoints.Remove(remove);
            }

            continue;

            bool InRange(VecD other) =>
                (other - point).LengthSquared <= minDistanceSquared;
        }

        if (availablePoints.Count == 1)
        {
            newPoints.Add(availablePoints[0]);
        }

        var finalData = new PointsVectorData(newPoints);

        return finalData;
    }

    public override Node CreateCopy() => new RemoveClosePointsNode();
}
