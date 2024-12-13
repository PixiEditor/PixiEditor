using System.Diagnostics;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.Views.Overlays.PathOverlay;

[DebuggerDisplay($"Position: {{{nameof(Position)}}}, Index: {{{nameof(Index)}}}")]
public class ShapePoint
{
    public VecF Position { get; set; }

    public int Index { get; set; }
    public Verb Verb { get; set; }

    public ShapePoint(VecF position, int index, Verb verb)
    {
        Position = position;
        Index = index;
        Verb = verb;
    }

    public void ConvertVerbToCubic()
    {
        if(Verb.IsEmptyVerb()) return;
        
        VecF[] points = ConvertVerbToCubicPoints();
        Verb = new Verb((PathVerb.Cubic, points, Verb.ConicWeight));
    }
    
    private VecF[] ConvertVerbToCubicPoints()
    {
        if (Verb.VerbType == PathVerb.Line)
        {
            return [Verb.From, Verb.ControlPoint1 ?? Verb.From, Verb.ControlPoint2 ?? Verb.To, Verb.To];
        }

        if (Verb.VerbType == PathVerb.Conic)
        {
            VecF mid1 = Verb.ControlPoint1 ?? Verb.From;
            
            float factor = 2 * Verb.ConicWeight / (1 + Verb.ConicWeight);

            VecF control1 = mid1;// + new VecF((mid1.X - Verb.From.X) * factor, (mid1.Y - Verb.From.Y) * factor);
            VecF control2 = Verb.To + new VecF((mid1.X - Verb.To.X) * factor, (mid1.Y - Verb.To.Y) * factor);
            
            return [Verb.From, control1, control2, Verb.To];
        }
        
        //TODO: Implement Quad to Cubic conversion
        return [Verb.From, Verb.ControlPoint1 ?? Verb.From, Verb.ControlPoint2 ?? Verb.To, Verb.To];
    }
}

[DebuggerDisplay($"{{{nameof(VerbType)}}}")]
public class Verb
{
    public PathVerb? VerbType { get; }

    public VecF From { get; set; }
    public VecF To { get; set; }
    public VecF? ControlPoint1 { get; set; }
    public VecF? ControlPoint2 { get; set; }
    public float ConicWeight { get; set; }

    public Verb()
    {
        VerbType = null;
    }
    
    public Verb((PathVerb verb, VecF[] points, float conicWeight) verbData)
    {
        VerbType = verbData.verb;
        From = verbData.points[0];
        To = GetPointFromVerb(verbData);
        ControlPoint1 = GetControlPoint(verbData, true);
        ControlPoint2 = GetControlPoint(verbData, false);
        ConicWeight = verbData.conicWeight;
    }
    
    public bool IsEmptyVerb()
    {
        return VerbType == null;
    }
    
    public static VecF GetPointFromVerb((PathVerb verb, VecF[] points, float conicWeight) data)
    {
        switch (data.verb)
        {
            case PathVerb.Move:
                return data.points[0];
            case PathVerb.Line:
                return data.points[1];
            case PathVerb.Quad:
                return data.points[2];
            case PathVerb.Cubic:
                return data.points[3];
            case PathVerb.Conic:
                return data.points[2];
            case PathVerb.Close:
                return data.points[0];
            case PathVerb.Done:
                return new VecF();
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public static VecF? GetControlPoint((PathVerb verb, VecF[] points, float conicWeight) data, bool first)
    {
        int index = first ? 1 : 2;
        switch (data.verb)
        {
            case PathVerb.Move:
                return null;
            case PathVerb.Line:
                return null;
            case PathVerb.Quad:
                return data.points[index];
            case PathVerb.Cubic:
                return data.points[index];
            case PathVerb.Conic:
                return data.points[index];
            case PathVerb.Close:
                return null;
            case PathVerb.Done:
                return null;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
