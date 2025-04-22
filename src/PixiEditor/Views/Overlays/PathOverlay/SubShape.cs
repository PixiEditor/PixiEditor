using System.Diagnostics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.Views.Overlays.PathOverlay;

[DebuggerDisplay($"Points: {{{nameof(Points)}}}, Closed: {{{nameof(IsClosed)}}}")]
public class SubShape
{
    private List<ShapePoint> points;

    public IReadOnlyList<ShapePoint> Points => points;
    public bool IsClosed { get; private set; }

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

    public void RemovePoint(int i)
    {
        bool isFirst = i == 0;
        bool isLast = i == points.Count - 1;

        if (!isFirst)
        {
            var previousPoint = GetPreviousPoint(i);
            var nextPoint = GetNextPoint(i);

            if (previousPoint?.Verb != null && nextPoint?.Verb != null)
            {
                previousPoint.Verb.To = nextPoint.Position;
            }

            for (int j = i + 1; j < points.Count; j++)
            {
                points[j].Index--;
            }
        }

        if (isLast)
        {
            points[^2].Verb = new Verb();
        }

        if (isFirst && points.Count > 2)
        {
            points[^1].Verb.To = points[1].Position;
        }

        points.RemoveAt(i);

        if (points.Count < 3)
        {
            IsClosed = false;
        }
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

    public void AppendPoint(VecF point)
    {
        if (points.Count == 0)
        {
            VecF[] data = new VecF[4];
            data[0] = VecF.Zero;
            data[1] = point;
            points.Add(new ShapePoint(point, 0, new Verb((PathVerb.Move, data, 0))));
        }
        else
        {
            var lastPoint = points[^1];
            VecF[] data = new VecF[4];
            data[0] = lastPoint.Position;
            data[1] = point;
            points.Add(new ShapePoint(point, lastPoint.Index + 1, new Verb((PathVerb.Line, data, 0))));
        }
    }

    public int InsertPointAt(VecF point, Verb pointVerb)
    {
        int indexOfVerb = this.points.FirstOrDefault(x => x.Verb == pointVerb)?.Index ?? -1;
        if (indexOfVerb == -1)
        {
            throw new ArgumentException("Verb not found in points list");
        }

        Verb onVerb = pointVerb;

        if (onVerb.VerbType is PathVerb.Quad or PathVerb.Conic)
        {
            this.points[indexOfVerb].ConvertVerbToCubic();
            onVerb = this.points[indexOfVerb].Verb;
        }

        var oldTo = onVerb.To;
        VecF[] data = new VecF[4];
        VecF insertPoint = point;

        if (onVerb.VerbType == PathVerb.Line)
        {
            onVerb.To = point;
            data = [onVerb.To, oldTo, VecF.Zero, VecF.Zero];
        }
        else
        {
            float t = VectorMath.GetNormalizedSegmentPosition(onVerb, point);
            VecD oldControlPoint1 = (VecD)onVerb.ControlPoint1.Value;
            VecD oldControlPoint2 = (VecD)onVerb.ControlPoint2.Value;

            // de Casteljau's algorithm

            var q0 = ((VecD)onVerb.From).Lerp(oldControlPoint1, t);
            var q1 = oldControlPoint1.Lerp(oldControlPoint2, t);
            var q2 = oldControlPoint2.Lerp((VecD)oldTo, t);

            var r0 = q0.Lerp(q1, t);
            var r1 = q1.Lerp(q2, t);

            var s0 = r0.Lerp(r1, t);

            onVerb.ControlPoint1 = (VecF)q0;
            onVerb.ControlPoint2 = (VecF)r0;

            onVerb.To = (VecF)s0;

            data = [(VecF)s0, (VecF)r1, (VecF)q2, oldTo];

            insertPoint = (VecF)s0;
        }

        this.points.Insert(indexOfVerb + 1,
            new ShapePoint(insertPoint, indexOfVerb + 1, new Verb((onVerb.VerbType.Value, data, 0))));

        for (int i = indexOfVerb + 2; i < this.points.Count; i++)
        {
            this.points[i].Index++;
        }

        return indexOfVerb + 1;
    }

    public VecD? GetClosestPointOnPath(VecD point, float maxDistanceInPixels)
    {
        for (int i = 0; i < points.Count; i++)
        {
            var currentPoint = points[i];

            VecD? closest = VectorMath.GetClosestPointOnSegment(point, currentPoint.Verb);
            if (closest != null && VecD.Distance(closest.Value, point) < maxDistanceInPixels)
            {
                return closest;
            }
        }

        return null;
    }

    public Verb? FindVerbContainingPoint(VecD point)
    {
        foreach (var shapePoint in points)
        {
            if (VectorMath.IsPointOnSegment(point, shapePoint.Verb))
            {
                return shapePoint.Verb;
            }
        }

        return null;
    }

    public void Close()
    {
        if (IsClosed)
        {
            return;
        }

        IsClosed = true;

        if (points.Count > 1)
        {
            VecF[] data = new VecF[4];
            data[0] = points[^1].Position;
            data[1] = points[0].Position;
            points.Add(new ShapePoint(points[0].Position, points[^1].Index + 1, new Verb((PathVerb.Line, data, 0))));
        }
    }
}
