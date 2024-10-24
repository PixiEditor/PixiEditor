using Drawie.Numerics;

namespace PixiEditor.Models.Controllers.InputDevice;

public class SnappingController
{
    public const double DefaultSnapDistance = 16;
    
    private string highlightedXAxis = string.Empty;
    private string highlightedYAxis = string.Empty;
    private VecD? highlightedPoint = null;
    private bool snappingEnabled = true;

    /// <summary>
    ///     Minimum distance that object has to be from snap point to snap to it. Expressed in pixels.
    /// </summary>
    public double SnapDistance { get; set; } = DefaultSnapDistance;

    public Dictionary<string, Func<double>> HorizontalSnapPoints { get; } = new();
    public Dictionary<string, Func<double>> VerticalSnapPoints { get; } = new();

    public string HighlightedXAxis
    {
        get => highlightedXAxis;
        set
        {
            highlightedXAxis = value;
            HorizontalHighlightChanged?.Invoke(value);
        }
    }

    public string HighlightedYAxis
    {
        get => highlightedYAxis;
        set
        {
            highlightedYAxis = value;
            VerticalHighlightChanged?.Invoke(value);
        }
    }

    public VecD? HighlightedPoint
    {
        get => highlightedPoint;
        set
        {
            highlightedPoint = value;
            HighlightedPointChanged?.Invoke(value);
        }
    }

    public bool SnappingEnabled
    {
        get => snappingEnabled;
        set
        {
            snappingEnabled = value;
            if (!value)
            {
                HighlightedXAxis = string.Empty;
                HighlightedYAxis = string.Empty;
                HighlightedPoint = null;
            }
        }
    }

    public event Action<string> HorizontalHighlightChanged;
    public event Action<string> VerticalHighlightChanged;
    public event Action<VecD?> HighlightedPointChanged;


    public double? SnapToHorizontal(double xPos, out string snapAxis)
    {
        if (!SnappingEnabled)
        {
            snapAxis = string.Empty;
            return null;
        }
        
        if (HorizontalSnapPoints.Count == 0)
        {
            snapAxis = string.Empty;
            return null;
        }

        snapAxis = HorizontalSnapPoints.First().Key;
        double closest = HorizontalSnapPoints.First().Value();
        foreach (var snapPoint in HorizontalSnapPoints)
        {
            if (Math.Abs(snapPoint.Value() - xPos) < Math.Abs(closest - xPos))
            {
                closest = snapPoint.Value();
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
        if (!SnappingEnabled)
        {
            snapAxisKey = string.Empty;
            return null;
        }
        
        if (VerticalSnapPoints.Count == 0)
        {
            snapAxisKey = string.Empty;
            return null;
        }

        snapAxisKey = VerticalSnapPoints.First().Key;
        double closest = VerticalSnapPoints.First().Value();
        foreach (var snapPoint in VerticalSnapPoints)
        {
            if (Math.Abs(snapPoint.Value() - yPos) < Math.Abs(closest - yPos))
            {
                closest = snapPoint.Value();
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
        HorizontalSnapPoints[identifier] = () => axisVector.X;
        VerticalSnapPoints[identifier] = () => axisVector.Y;
    }

    public void AddBounds(string identifier, Func<RectD> tightBounds)
    {
        HorizontalSnapPoints[$"{identifier}.center"] = () => tightBounds().Center.X;
        VerticalSnapPoints[$"{identifier}.center"] = () => tightBounds().Center.Y;

        HorizontalSnapPoints[$"{identifier}.left"] = () => tightBounds().Left;
        VerticalSnapPoints[$"{identifier}.top"] = () => tightBounds().Top;

        HorizontalSnapPoints[$"{identifier}.right"] = () => tightBounds().Right;
        VerticalSnapPoints[$"{identifier}.bottom"] = () => tightBounds().Bottom;
    }

    /// <summary>
    ///     Removes all snap points with root identifier. All identifiers that start with root will be removed.
    /// </summary>
    /// <param name="id">Root identifier of snap points to remove.</param>
    public void RemoveAll(string id)
    {
        var toRemoveHorizontal = HorizontalSnapPoints.Keys.Where(x => x.StartsWith(id)).ToList();
        var toRemoveVertical = VerticalSnapPoints.Keys.Where(x => x.StartsWith(id)).ToList();

        foreach (var key in toRemoveHorizontal)
        {
            HorizontalSnapPoints.Remove(key);
        }

        foreach (var key in toRemoveVertical)
        {
            VerticalSnapPoints.Remove(key);
        }
    }

    public VecD GetSnapDeltaForPoints(VecD[] points, out string xAxis, out string yAxis)
    {
        if (!SnappingEnabled)
        {
            xAxis = string.Empty;
            yAxis = string.Empty;
            return VecD.Zero;
        }
        
        bool hasXSnap = false;
        bool hasYSnap = false;
        VecD snapDelta = VecD.Zero;

        string snapAxisX = string.Empty;
        string snapAxisY = string.Empty;

        foreach (var point in points)
        {
            double? snapX = SnapToHorizontal(point.X, out string newSnapAxisX);
            double? snapY = SnapToVertical(point.Y, out string newSnapAxisY);

            if (snapX is not null && !hasXSnap)
            {
                snapDelta += new VecD(snapX.Value - point.X, 0);
                snapAxisX = newSnapAxisX;
                hasXSnap = true;
            }

            if (snapY is not null && !hasYSnap)
            {
                snapDelta += new VecD(0, snapY.Value - point.Y);
                snapAxisY = newSnapAxisY;
                hasYSnap = true;
            }

            if (hasXSnap && hasYSnap)
            {
                break;
            }
        }

        xAxis = snapAxisX;
        yAxis = snapAxisY;

        return snapDelta;
    }

    public VecD GetSnapPoint(VecD pos, out string xAxis, out string yAxis)
    {
        if (!SnappingEnabled)
        {
            xAxis = string.Empty;
            yAxis = string.Empty;
            return pos;
        }
        
        double? snapX = SnapToHorizontal(pos.X, out string snapAxisX);
        double? snapY = SnapToVertical(pos.Y, out string snapAxisY);

        xAxis = snapAxisX;
        yAxis = snapAxisY;
        
        return new VecD(snapX ?? pos.X, snapY ?? pos.Y);
    }

    public VecD GetSnapDeltaForPoint(VecD pos, out string xAxis, out string yAxis)
    {
        if (!SnappingEnabled)
        {
            xAxis = string.Empty;
            yAxis = string.Empty;
            return VecD.Zero;
        }
        
        double? snapX = SnapToHorizontal(pos.X, out string snapAxisX);
        double? snapY = SnapToVertical(pos.Y, out string snapAxisY);

        xAxis = snapAxisX;
        yAxis = snapAxisY;

        VecD snappedPos = new VecD(snapX ?? pos.X, snapY ?? pos.Y);

        return snappedPos - pos;
    }
}
