using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;

[NodeInfo("RemoveClosePoints", "REMOVE_CLOSE_POINTS", Category = "OPERATIONS")]
public class RemoveClosePointsNode : Node
{
    public OutputProperty<PointList> Output { get; }
    
    public InputProperty<PointList> Input { get; }
    
    public InputProperty<double> MinDistance { get; }

    public InputProperty<int> Seed { get; }

    public RemoveClosePointsNode()
    {
        Output = CreateOutput("Output", "POINTS", PointList.Empty);
        Input = CreateInput("Input", "POINTS", PointList.Empty);
        MinDistance = CreateInput("MinDistance", "MIN_DISTANCE", 0d);
        Seed = CreateInput("Seed", "SEED", 0);
    }
    
    protected override Texture? OnExecute(RenderingContext context)
    {
        var distance = MinDistance.Value;

        if (distance == 0)
        {
            Output.Value = Input.Value;
            return null;
        }

        var availablePoints = Input.Value.Distinct().ToList();
        var newPoints = new PointList(availablePoints.Count) { HashValue = HashCode.Combine(Input.Value.HashValue, MinDistance.Value, Seed.Value) };

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
            bool InRange(VecD other) => (other.Multiply(documentSize) - point.Multiply(documentSize)).Length <= minDistance;
        }

        if (availablePoints.Count == 1)
        {
            newPoints.Add(availablePoints[0]);
        }
        
        Output.Value = newPoints;
        
        return null;

    }

    public override Node CreateCopy() => new RemoveClosePointsNode();
}
