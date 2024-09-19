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
    
    public string HighlightedXAxis { get; set; } = string.Empty;
    public string HighlightedYAxis { get; set; } = string.Empty;
    
    
    public double? SnapToHorizontal(double xPos, out string snapAxis)
    {
        if (HorizontalSnapPoints.Count == 0)
        {
            snapAxis = string.Empty;
            return null;
        }
        
        snapAxis = HorizontalSnapPoints.First().Key;
        double closest = HorizontalSnapPoints.First().Value;
        foreach (var snapPoint in HorizontalSnapPoints)
        {
            if (Math.Abs(snapPoint.Value - xPos) < Math.Abs(closest - xPos))
            {
                closest = snapPoint.Value;
                snapAxis = snapPoint.Key;
            }
        }
        
        if (Math.Abs(closest - xPos) > SnapDistance)
        {
            snapAxis = string.Empty;
            return null;
        }
        
        return closest;
    }
    
    public double? SnapToVertical(double yPos, out string snapAxisKey)
    {
        if (VerticalSnapPoints.Count == 0)
        {
            snapAxisKey = string.Empty;
            return null;
        }

        snapAxisKey = VerticalSnapPoints.First().Key;
        double closest = VerticalSnapPoints.First().Value;
        foreach (var snapPoint in VerticalSnapPoints)
        {
            if (Math.Abs(snapPoint.Value - yPos) < Math.Abs(closest - yPos))
            {
                closest = snapPoint.Value;
                snapAxisKey = snapPoint.Key;
            }
        }
        
        if (Math.Abs(closest - yPos) > SnapDistance)
        {
            snapAxisKey = string.Empty;
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
