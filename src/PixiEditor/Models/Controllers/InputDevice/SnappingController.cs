using PixiEditor.Numerics;

namespace PixiEditor.Models.Controllers.InputDevice;

public class SnappingController
{
    public const double DefaultSnapDistance = 16;
    
    /// <summary>
    ///     Minimum distance that object has to be from snap point to snap to it. Expressed in pixels.
    /// </summary>
    public double SnapDistance { get; set; } = DefaultSnapDistance;

    public Dictionary<string, double> HorizontalSnapPoints { get; } = new();
    public Dictionary<string, double> VerticalSnapPoints { get; } = new(); 
    
    
    public double? SnapToHorizontal(double xPos)
    {
        if (HorizontalSnapPoints.Count == 0)
        {
            return null;
        }

        double closest = HorizontalSnapPoints.First().Value;
        foreach (double snapPoint in HorizontalSnapPoints.Values)
        {
            if (Math.Abs(snapPoint - xPos) < Math.Abs(closest - xPos))
            {
                closest = snapPoint;
            }
        }
        
        if (Math.Abs(closest - xPos) > SnapDistance)
        {
            return null;
        }

        return closest;
    }
    
    public double? SnapToVertical(double yPos)
    {
        if (VerticalSnapPoints.Count == 0)
        {
            return null;
        }

        double closest = VerticalSnapPoints.First().Value;
        foreach (double snapPoint in VerticalSnapPoints.Values)
        {
            if (Math.Abs(snapPoint - yPos) < Math.Abs(closest - yPos))
            {
                closest = snapPoint;
            }
        }
        
        if (Math.Abs(closest - yPos) > SnapDistance)
        {
            return null;
        }

        return closest;
    }

    public void AddXYAxis(string identifier, VecD axisVector)
    {
        HorizontalSnapPoints[identifier] = axisVector.X;
        VerticalSnapPoints[identifier] = axisVector.Y;
    }
}
