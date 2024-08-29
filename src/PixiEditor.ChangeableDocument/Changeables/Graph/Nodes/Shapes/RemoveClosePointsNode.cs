using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;
using ShapeData = PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data.ShapeData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes;

[NodeInfo("RemoveClosePoints", "REMOVE_CLOSE_POINTS", Category = "SHAPE")]
public class RemoveClosePointsNode : ShapeNode<PointsData>
{
    public InputProperty<PointsData> Input { get; }

    public InputProperty<double> MinDistance { get; }

    public InputProperty<int> Seed { get; }

    public RemoveClosePointsNode()
    {
        Input = CreateInput<PointsData>("Input", "POINTS", null);
        MinDistance = CreateInput("MinDistance", "MIN_DISTANCE", 0d);
        Seed = CreateInput("Seed", "SEED", 0);
    }

    protected override PointsData? GetShapeData(RenderingContext context)
    {
        var data = Input.Value;

        var distance = MinDistance.Value;

        if (distance == 0 || data == null || data.Points == null)
        {
            return null;
        }

        var availablePoints = data.Points.Distinct().ToList();
        List<VecD> newPoints = new List<VecD>();
        
        var minDistance = MinDistance.Value;
        var documentSize = context.DocumentSize;

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
                (other - point).Length <= minDistance;
        }

        if (availablePoints.Count == 1)
        {
            newPoints.Add(availablePoints[0]);
        }

        var finalData = new PointsData(newPoints);

        return finalData;
    }

    public override Node CreateCopy() => new RemoveClosePointsNode();
}
