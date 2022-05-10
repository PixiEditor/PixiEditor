namespace ChunkyImageLib.DataHolders;
public struct ShapeCorners
{
    public ShapeCorners(Vector2d center, Vector2d size, double angle)
    {
        TopLeft = center - size / 2;
        TopRight = center + new Vector2d(size.X / 2, -size.Y / 2);
        BottomRight = center + size / 2;
        BottomLeft = center + new Vector2d(-size.X / 2, size.Y / 2);
    }
    public Vector2d TopLeft { get; set; }
    public Vector2d TopRight { get; set; }
    public Vector2d BottomLeft { get; set; }
    public Vector2d BottomRight { get; set; }
    public bool IsLegal
    {
        get
        {
            var top = TopLeft - TopRight;
            var right = TopRight - BottomRight;
            var bottom = BottomRight - BottomLeft;
            var left = BottomLeft - TopLeft;
            var topRight = Math.Sign(top.Cross(right));
            return topRight == Math.Sign(right.Cross(bottom)) && topRight == Math.Sign(bottom.Cross(left)) && topRight == Math.Sign(left.Cross(top));
        }
    }
    public bool HasNaNOrInfinity => TopLeft.IsNaNOrInfinity() || TopRight.IsNaNOrInfinity() || BottomLeft.IsNaNOrInfinity() || BottomRight.IsNaNOrInfinity();
    public Vector2d RectSize => new((TopLeft - TopRight).Length, (TopLeft - BottomLeft).Length);
    public Vector2d RectCenter => (TopLeft - BottomRight) / 2 + BottomRight;
    public double RectRotation =>
        (TopLeft - TopRight).Cross(TopLeft - BottomLeft) > 0 ?
        RectSize.CCWAngleTo(BottomRight - TopLeft) :
        RectSize.CCWAngleTo(BottomLeft - TopRight);
}
