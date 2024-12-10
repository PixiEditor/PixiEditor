using System.Diagnostics;
using Drawie.Numerics;

namespace PixiEditor.Views.Overlays.PathOverlay;

[DebuggerDisplay($"Points: {{{nameof(Points)}}}, Closed: {{{nameof(IsClosed)}}}")]
public class SubShape
{
    private List<ShapePoint> points;

    public IReadOnlyList<ShapePoint> Points => points;
    public bool IsClosed { get; }

    public ShapePoint? GetNextPoint(int nextToIndex)
    {
        if (nextToIndex + 1 < points.Count)
        {
            return points[nextToIndex + 1];
        }

        return IsClosed ? points[0] : null;
    }

    public ShapePoint? GetPreviousPoint(int previousToIndex)
    {
        if (previousToIndex - 1 >= 0)
        {
            return points[previousToIndex - 1];
        }

        return IsClosed ? points[^1] : null;
    }

    public SubShape(List<ShapePoint> points, bool isClosed)
    {
        this.points = new List<ShapePoint>(points);
        IsClosed = isClosed;
    }

    public void SetPointPosition(int i, VecF newPos, bool updateControlPoints)
    {
        var shapePoint = points[i];
        var oldPos = shapePoint.Position;
        VecF delta = newPos - oldPos;
        shapePoint.Position = newPos;
        shapePoint.Verb.From = newPos;

        if (updateControlPoints)
        {
            if (shapePoint.Verb.ControlPoint1 != null)
            {
                shapePoint.Verb.ControlPoint1 = shapePoint.Verb.ControlPoint1.Value + delta;
            }
            
        }

        var previousPoint = GetPreviousPoint(i);

        if (previousPoint?.Verb != null && previousPoint.Verb.To == oldPos)
        {
            previousPoint.Verb.To = newPos;
            
            if (updateControlPoints)
            {
                if (previousPoint.Verb.ControlPoint2 != null)
                {
                    previousPoint.Verb.ControlPoint2 = previousPoint.Verb.ControlPoint2.Value + delta;
                }
            }
        }
    }
}
