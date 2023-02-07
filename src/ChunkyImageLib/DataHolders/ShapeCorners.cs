using PixiEditor.DrawingApi.Core.Numerics;

namespace ChunkyImageLib.DataHolders;
public struct ShapeCorners
{
    private const double epsilon = 0.001;
    public ShapeCorners(VecD center, VecD size)
    {
        TopLeft = center - size / 2;
        TopRight = center + new VecD(size.X / 2, -size.Y / 2);
        BottomRight = center + size / 2;
        BottomLeft = center + new VecD(-size.X / 2, size.Y / 2);
    }
    public ShapeCorners(RectD rect)
    {
        TopLeft = rect.TopLeft;
        TopRight = rect.TopRight;
        BottomRight = rect.BottomRight;
        BottomLeft = rect.BottomLeft;
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
            if (HasNaNOrInfinity)
                return false;
            var top = TopLeft - TopRight;
            var right = TopRight - BottomRight;
            var bottom = BottomRight - BottomLeft;
            var left = BottomLeft - TopLeft;
            var topRight = Math.Sign(top.Cross(right));
            return topRight == Math.Sign(right.Cross(bottom)) && topRight == Math.Sign(bottom.Cross(left)) && topRight == Math.Sign(left.Cross(top));
        }
    }

    /// <summary>
    /// Checks if two or more corners are in the same position
    /// </summary>
    public bool IsPartiallyDegenerate
    {
        get
        {
            Span<VecD> lengths = stackalloc[] 
            {
                TopLeft - TopRight,
                TopRight - BottomRight,
                BottomRight - BottomLeft,
                BottomLeft - TopLeft,
                TopLeft - BottomRight,
                TopRight - BottomLeft
            };
            foreach (VecD vec in lengths)
            {
                if (vec.LengthSquared < epsilon * epsilon)
                    return true;
            }
            return false;
        }
    }
    public bool HasNaNOrInfinity => TopLeft.IsNaNOrInfinity() || TopRight.IsNaNOrInfinity() || BottomLeft.IsNaNOrInfinity() || BottomRight.IsNaNOrInfinity();
    public bool IsRect => Math.Abs((TopLeft - BottomRight).Length - (TopRight - BottomLeft).Length) < epsilon;
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
    public RectD AABBBounds
    {
        get
        {
            double minX = Math.Min(Math.Min(TopLeft.X, TopRight.X), Math.Min(BottomLeft.X, BottomRight.X));
            double minY = Math.Min(Math.Min(TopLeft.Y, TopRight.Y), Math.Min(BottomLeft.Y, BottomRight.Y));
            double maxX = Math.Max(Math.Max(TopLeft.X, TopRight.X), Math.Max(BottomLeft.X, BottomRight.X));
            double maxY = Math.Max(Math.Max(TopLeft.Y, TopRight.Y), Math.Max(BottomLeft.Y, BottomRight.Y));
            return RectD.FromTwoPoints(new VecD(minX, minY), new VecD(maxX, maxY));
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

    public ShapeCorners AsMirroredAcrossHorAxis(int horAxisY) => new ShapeCorners
    {
        BottomLeft = BottomLeft.ReflectY(horAxisY),
        BottomRight = BottomRight.ReflectY(horAxisY),
        TopLeft = TopLeft.ReflectY(horAxisY),
        TopRight = TopRight.ReflectY(horAxisY)
    };

    public ShapeCorners AsMirroredAcrossVerAxis(int verAxisX) => new ShapeCorners
    {
        BottomLeft = BottomLeft.ReflectX(verAxisX),
        BottomRight = BottomRight.ReflectX(verAxisX),
        TopLeft = TopLeft.ReflectX(verAxisX),
        TopRight = TopRight.ReflectX(verAxisX)
    };

    public ShapeCorners AsRotated(double angle, VecD around) => new ShapeCorners
    {
        BottomLeft = BottomLeft.Rotate(angle, around),
        BottomRight = BottomRight.Rotate(angle, around),
        TopLeft = TopLeft.Rotate(angle, around),
        TopRight = TopRight.Rotate(angle, around)
    };

    public ShapeCorners AsTranslated(VecD delta) => new ShapeCorners
    {
        BottomLeft = BottomLeft + delta,
        BottomRight = BottomRight + delta,
        TopLeft = TopLeft + delta,
        TopRight = TopRight + delta
    };

    public static bool operator !=(ShapeCorners left, ShapeCorners right) => !(left == right);
    public static bool operator == (ShapeCorners left, ShapeCorners right)
    {
        return 
           left.TopLeft == right.TopLeft &&
           left.TopRight == right.TopRight &&
           left.BottomLeft == right.BottomLeft &&
           left.BottomRight == right.BottomRight;
    }

    public bool AlmostEquals(ShapeCorners other, double epsilon = 0.001)
    {
        return
            TopLeft.AlmostEquals(other.TopLeft, epsilon) &&
            TopRight.AlmostEquals(other.TopRight, epsilon) &&
            BottomLeft.AlmostEquals(other.BottomLeft, epsilon) &&
            BottomRight.AlmostEquals(other.BottomRight, epsilon);
    }
}
