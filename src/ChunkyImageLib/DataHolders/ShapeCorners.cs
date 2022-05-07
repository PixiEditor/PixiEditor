namespace ChunkyImageLib.DataHolders;
public struct ShapeCorners
{
    public Vector2d TopLeft { get; set; }
    public Vector2d TopRight { get; set; }
    public Vector2d BottomLeft { get; set; }
    public Vector2d BottomRight { get; set; }
    public bool HasNaNOrInfinity => TopLeft.IsNaNOrInfinity() || TopRight.IsNaNOrInfinity() || BottomLeft.IsNaNOrInfinity() || BottomRight.IsNaNOrInfinity();
    public double TopWidth => (TopRight - TopLeft).Length;
    public double LeftHeight => (TopLeft - BottomLeft).Length;
}
