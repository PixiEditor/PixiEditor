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
                break;
            case PathVerb.Conic:
                break;
            case PathVerb.Cubic:
                break;
            case PathVerb.Close:
                break;
            case PathVerb.Done:
                break;
            case null:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return null;
    }
    
    public static bool IsPointOnSegment(VecF point, Verb shapePointVerb)
    {
        if (shapePointVerb.IsEmptyVerb()) return false;

        switch (shapePointVerb.VerbType)
        {
            case PathVerb.Move:
                return point == shapePointVerb.From;
            case PathVerb.Line:
                return IsPointOnLine(point, shapePointVerb.From, shapePointVerb.To);
            case PathVerb.Quad:
                break;
            case PathVerb.Conic:
                break;
            case PathVerb.Cubic:
                break;
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
    
    public static bool IsPointOnLine(VecF point, VecF start, VecF end)
    {
        VecF startToPoint = point - start;
        VecF startToEnd = end - start;
        
        double sqrtMagnitudeToEnd = Math.Pow(startToEnd.X, 2) + Math.Pow(startToEnd.Y, 2);
        
        double dot = startToPoint.X * startToEnd.X + startToPoint.Y * startToEnd.Y;
        var t = dot / sqrtMagnitudeToEnd;
        
        return t is >= 0 and <= 1;
    }
}
