using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.Views.Overlays.PathOverlay;

internal static class VectorMath
{
    public static VecD? GetClosestPointOnSegment(VecD point, Verb verb)
    {
        if (verb == null || verb.IsEmptyVerb()) return null;

        switch (verb.VerbType)
        {
            case PathVerb.Move:
                return (VecD)verb.From;
            case PathVerb.Line:
                return ClosestPointOnLine((VecD)verb.From, (VecD)verb.To, point);
            case PathVerb.Quad:
                return GetClosestPointOnQuad(point, (VecD)verb.From, (VecD)(verb.ControlPoint1 ?? verb.From),
                    (VecD)verb.To);
            case PathVerb.Conic:
                return GetClosestPointOnConic(point, (VecD)verb.From, (VecD)(verb.ControlPoint1 ?? verb.From),
                    (VecD)verb.To,
                    verb.ConicWeight);
            case PathVerb.Cubic:
                return GetClosestPointOnCubic(point, (VecD)verb.From, (VecD)(verb.ControlPoint1 ?? verb.From),
                    (VecD)(verb.ControlPoint2 ?? verb.To), (VecD)verb.To);
            case PathVerb.Close:
                return (VecD)verb.From;
            case PathVerb.Done:
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return null;
    }

    public static bool IsPointOnSegment(VecD point, Verb shapePointVerb)
    {
        if (shapePointVerb.IsEmptyVerb()) return false;

        switch (shapePointVerb.VerbType)
        {
            case PathVerb.Move:
                return Math.Abs(point.X - shapePointVerb.From.X) < 0.0001 &&
                       Math.Abs(point.Y - shapePointVerb.From.Y) < 0.0001;
            case PathVerb.Line:
                return IsPointOnLine(point, (VecD)shapePointVerb.From, (VecD)shapePointVerb.To);
            case PathVerb.Quad:
                return IsPointOnQuad(point, (VecD)shapePointVerb.From,
                    (VecD)(shapePointVerb.ControlPoint1 ?? shapePointVerb.From),
                    (VecD)shapePointVerb.To);
            case PathVerb.Conic:
                return IsPointOnConic(point, (VecD)shapePointVerb.From,
                    (VecD)(shapePointVerb.ControlPoint1 ?? shapePointVerb.From),
                    (VecD)shapePointVerb.To, shapePointVerb.ConicWeight);
            case PathVerb.Cubic:
                return IsPointOnCubic(point, (VecD)shapePointVerb.From,
                    (VecD)(shapePointVerb.ControlPoint1 ?? shapePointVerb.From),
                    (VecD)(shapePointVerb.ControlPoint2 ?? shapePointVerb.To), (VecD)shapePointVerb.To);
            case PathVerb.Close:
                break;
            case PathVerb.Done:
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    public static VecD ClosestPointOnLine(VecD start, VecD end, VecD point)
    {
        VecD startToPoint = point - start;
        VecD startToEnd = end - start;

        double sqrtMagnitudeToEnd = Math.Pow(startToEnd.X, 2) + Math.Pow(startToEnd.Y, 2);

        double dot = startToPoint.X * startToEnd.X + startToPoint.Y * startToEnd.Y;
        var t = dot / sqrtMagnitudeToEnd;

        if (t < 0) return start;
        if (t > 1) return end;

        return start + startToEnd * t;
    }

    public static bool IsPointOnLine(VecD point, VecD start, VecD end)
    {
        return Math.Abs(VecD.Distance(start, point) + VecD.Distance(end, point) - VecD.Distance(start, end)) < 0.001f;
    }

    public static VecD GetClosestPointOnQuad(VecD point, VecD start, VecD controlPoint, VecD end)
    {
        return FindClosestPointBruteForce(point, (t) => QuadraticBezier(start, controlPoint, end, t));
    }

    public static VecD GetClosestPointOnCubic(VecD point, VecD start, VecD controlPoint1, VecD controlPoint2, VecD end)
    {
        return FindClosestPointBruteForce(point, (t) => CubicBezier(start, controlPoint1, controlPoint2, end, t));
    }

    public static VecD GetClosestPointOnConic(VecD point, VecD start, VecD controlPoint, VecD end, float weight)
    {
        return FindClosestPointBruteForce(point, (t) => ConicBezier(start, controlPoint, end, weight, t));
    }

    public static bool IsPointOnQuad(VecD point, VecD start, VecD controlPoint, VecD end)
    {
        return IsPointOnPath(point, (t) => QuadraticBezier(start, controlPoint, end, t));
    }

    public static bool IsPointOnCubic(VecD point, VecD start, VecD controlPoint1, VecD controlPoint2, VecD end)
    {
        return IsPointOnPath(point, (t) => CubicBezier(start, controlPoint1, controlPoint2, end, t));
    }

    public static bool IsPointOnConic(VecD point, VecD start, VecD controlPoint, VecD end, float weight)
    {
        return IsPointOnPath(point, (t) => ConicBezier(start, controlPoint, end, weight, t));
    }

    /// <summary>
    ///     Finds value from 0 to 1 that represents the position of point on the segment.
    /// </summary>
    /// <param name="onVerb">Verb that represents the segment.</param>
    /// <param name="point">Point that is on the segment.</param>
    /// <returns>Value from 0 to 1 that represents the position of point on the segment.</returns>
    public static float GetNormalizedSegmentPosition(Verb onVerb, VecF point)
    {
        if (onVerb.IsEmptyVerb()) return 0;

        if (onVerb.VerbType == PathVerb.Cubic)
        {
            return (float)FindNormalizedSegmentPositionBruteForce(point, (t) =>
                CubicBezier((VecD)onVerb.From, (VecD)(onVerb.ControlPoint1 ?? onVerb.From),
                    (VecD)(onVerb.ControlPoint2 ?? onVerb.To), (VecD)onVerb.To, t));
        }
        
        throw new NotImplementedException();
    }

    private static VecD FindClosestPointBruteForce(VecD point, Func<double, VecD> func, double step = 0.001)
    {
        double minDistance = double.MaxValue;
        VecD closestPoint = new VecD();
        for (double t = 0; t <= 1; t += step)
        {
            VecD currentPoint = func(t);
            double distance = VecD.Distance(point, currentPoint);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = currentPoint;
            }
        }

        return closestPoint;
    }

    private static double FindNormalizedSegmentPositionBruteForce(VecF point, Func<double, VecD> func,
        double step = 0.001)
    {
        double minDistance = float.MaxValue;
        double closestT = 0;
        for (double t = 0; t <= 1; t += step)
        {
            VecD currentPoint = func(t);
            float distance = (point - currentPoint).Length;
            if (distance < minDistance)
            {
                minDistance = distance;
                closestT = t;
            }
        }

        return closestT;
    }

    private static bool IsPointOnPath(VecD point, Func<double, VecD> func, double step = 0.001)
    {
        for (double t = 0; t <= 1; t += step)
        {
            VecD currentPoint = func(t);
            if (VecD.Distance(point, currentPoint) < 0.1)
            {
                return true;
            }
        }

        return false;
    }

    private static VecD QuadraticBezier(VecD start, VecD control, VecD end, double t)
    {
        double x = Math.Pow(1 - t, 2) * start.X + 2 * (1 - t) * t * control.X + Math.Pow(t, 2) * end.X;
        double y = Math.Pow(1 - t, 2) * start.Y + 2 * (1 - t) * t * control.Y + Math.Pow(t, 2) * end.Y;
        return new VecD(x, y);
    }

    private static VecD CubicBezier(VecD start, VecD control1, VecD control2, VecD end, double t)
    {
        double x = Math.Pow(1 - t, 3) * start.X + 3 * Math.Pow(1 - t, 2) * t * control1.X +
                   3 * (1 - t) * Math.Pow(t, 2) * control2.X + Math.Pow(t, 3) * end.X;
        double y = Math.Pow(1 - t, 3) * start.Y + 3 * Math.Pow(1 - t, 2) * t * control1.Y +
                   3 * (1 - t) * Math.Pow(t, 2) * control2.Y + Math.Pow(t, 3) * end.Y;
        return new VecD(x, y);
    }

    private static VecD ConicBezier(VecD start, VecD control, VecD end, float weight, double t)
    {
        double b0 = (1 - t) * (1 - t);
        double b1 = 2 * t * (1 - t);
        double b2 = t * t;

        VecD numerator = (start * b0) + (control * b1 * weight) + (end * b2);

        double denominator = b0 + (b1 * weight) + b2;

        return numerator / denominator;
    }
}
