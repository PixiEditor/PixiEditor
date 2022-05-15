namespace ChunkyImageLib.DataHolders;
public struct ShapeCorners
{
    public ShapeCorners(VecD center, VecD size, double angle)
    {
        TopLeft = center - size / 2;
        TopRight = center + new VecD(size.X / 2, -size.Y / 2);
        BottomRight = center + size / 2;
        BottomLeft = center + new VecD(-size.X / 2, size.Y / 2);
    }
    public ShapeCorners(VecD topLeft, VecD size)
    {
        TopLeft = topLeft;
        TopRight = new(topLeft.X + size.X, topLeft.Y);
        BottomRight = topLeft + size;
        BottomLeft = new(topLeft.X, topLeft.Y + size.Y);
    }
    public VecD TopLeft { get; set; }
    public VecD TopRight { get; set; }
    public VecD BottomLeft { get; set; }
    public VecD BottomRight { get; set; }
    public bool IsInverted
    {
        get
        {
            var top = TopLeft - TopRight;
            var right = TopRight - BottomRight;
            var bottom = BottomRight - BottomLeft;
            var left = BottomLeft - TopLeft;
            return Math.Sign(top.Cross(right)) + Math.Sign(right.Cross(bottom)) + Math.Sign(bottom.Cross(left)) + Math.Sign(left.Cross(top)) < 0;
        }
    }
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
    public bool IsRect => Math.Abs((TopLeft - BottomRight).Length - (TopRight - BottomLeft).Length) < 0.001;
    public VecD RectSize => new((TopLeft - TopRight).Length, (TopLeft - BottomLeft).Length);
    public VecD RectCenter => (TopLeft - BottomRight) / 2 + BottomRight;
    public double RectRotation =>
        (TopLeft - TopRight).Cross(TopLeft - BottomLeft) > 0 ?
        RectSize.CCWAngleTo(BottomRight - TopLeft) :
        RectSize.CCWAngleTo(BottomLeft - TopRight);
    public bool IsSnappedToPixels
    {
        get
        {
            double epsilon = 0.01;
            return
                (TopLeft - TopLeft.Round()).TaxicabLength < epsilon &&
                (TopRight - TopRight.Round()).TaxicabLength < epsilon &&
                (BottomLeft - BottomLeft.Round()).TaxicabLength < epsilon &&
                (BottomRight - BottomRight.Round()).TaxicabLength < epsilon;
        }
    }
    public bool IsPointInside(VecD point)
    {
        var top = TopLeft - TopRight;
        var right = TopRight - BottomRight;
        var bottom = BottomRight - BottomLeft;
        var left = BottomLeft - TopLeft;

        var deltaTopLeft = point - TopLeft;
        var deltaTopRight = point - TopRight;
        var deltaBottomRight = point - BottomRight;
        var deltaBottomLeft = point - BottomLeft;

        if (deltaTopRight.IsNaNOrInfinity() || deltaTopLeft.IsNaNOrInfinity() || deltaBottomRight.IsNaNOrInfinity() || deltaBottomRight.IsNaNOrInfinity())
            return false;

        var crossTop = Math.Sign(top.Cross(deltaTopLeft));
        var crossRight = Math.Sign(right.Cross(deltaTopRight));
        var crossBottom = Math.Sign(bottom.Cross(deltaBottomRight));
        var crossLeft = Math.Sign(left.Cross(deltaBottomLeft));

        return crossTop == crossRight && crossTop == crossLeft && crossTop == crossBottom;
    }

    public ShapeCorners AsMirroredAcrossHorAxis(int horAxisY) => this with
    {
        BottomLeft = BottomLeft.ReflectY(horAxisY),
        BottomRight = BottomRight.ReflectY(horAxisY),
        TopLeft = TopLeft.ReflectY(horAxisY),
        TopRight = TopRight.ReflectY(horAxisY),
    };
    public ShapeCorners AsMirroredAcrossVerAxis(int verAxisX) => this with
    {
        BottomLeft = BottomLeft.ReflectX(verAxisX),
        BottomRight = BottomRight.ReflectX(verAxisX),
        TopLeft = TopLeft.ReflectX(verAxisX),
        TopRight = TopRight.ReflectX(verAxisX),
    };
}
