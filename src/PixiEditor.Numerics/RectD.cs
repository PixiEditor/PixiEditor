namespace PixiEditor.Numerics;
public struct RectD : IEquatable<RectD>
{
    public static RectD Empty { get; } = new RectD();

    private double left;
    private double top;
    private double right;
    private double bottom;

    public double Left { readonly get => left; set => left = value; }
    public double Top { readonly get => top; set => top = value; }
    public double Right { readonly get => right; set => right = value; }
    public double Bottom { readonly get => bottom; set => bottom = value; }
    public double X { readonly get => left; set => left = value; }
    public double Y { readonly get => top; set => top = value; }
    public bool HasNaNOrInfinity =>
        double.IsNaN(left) || double.IsInfinity(left) ||
        double.IsNaN(right) || double.IsInfinity(right) ||
        double.IsNaN(top) || double.IsInfinity(top) ||
        double.IsNaN(bottom) || double.IsInfinity(bottom);

    public VecD Pos
    {
        readonly get => new VecD(left, top);
        set
        {
            right = (right - left) + value.X;
            bottom = (bottom - top) + value.Y;
            left = value.X;
            top = value.Y;
        }
    }
    public VecD TopLeft
    {
        readonly get => new VecD(left, top);
        set
        {
            left = value.X;
            top = value.Y;
        }
    }
    public VecD TopRight
    {
        readonly get => new VecD(right, top);
        set
        {
            right = value.X;
            top = value.Y;
        }
    }
    public VecD BottomLeft
    {
        readonly get => new VecD(left, bottom);
        set
        {
            left = value.X;
            bottom = value.Y;
        }
    }
    public VecD BottomRight
    {
        readonly get => new VecD(right, bottom);
        set
        {
            right = value.X;
            bottom = value.Y;
        }
    }
    public VecD Size
    {
        readonly get => new VecD(right - left, bottom - top);
        set
        {
            right = left + value.X;
            bottom = top + value.Y;
        }
    }
    public VecD Center { get => new VecD((left + right) / 2.0, (top + bottom) / 2.0); }
    public double Width { readonly get => right - left; set => right = left + value; }
    public double Height { readonly get => bottom - top; set => bottom = top + value; }
    public readonly bool IsZeroArea => left == right || top == bottom;
    public readonly bool IsZeroOrNegativeArea => left >= right || top >= bottom;
    public RectD()
    {
        left = 0d;
        top = 0d;
        right = 0d;
        bottom = 0d;
    }

    public RectD(VecD pos, VecD size)
    {
        left = pos.X;
        top = pos.Y;
        right = pos.X + size.X;
        bottom = pos.Y + size.Y;
    }

    public RectD(double x, double y, double width, double height)
    {
        left = x;
        top = y;
        right = x + width;
        bottom = y + height;
    }
    
    public static RectD Create(VecI pos, VecI size)
    {
        return new RectD(pos.X, pos.Y, size.X, size.Y);
    }
    
    public static RectD FromSides(double left, double right, double top, double bottom)
    {
        return new RectD()
        {
            Left = left,
            Right = right,
            Top = top,
            Bottom = bottom
        };
    }

    public static RectD FromTwoPoints(VecD point, VecD opposite)
    {
        return new RectD()
        {
            Left = Math.Min(point.X, opposite.X),
            Right = Math.Max(point.X, opposite.X),
            Top = Math.Min(point.Y, opposite.Y),
            Bottom = Math.Max(point.Y, opposite.Y)
        };
    }

    public static RectD FromCenterAndSize(VecD center, VecD size)
    {
        return new RectD()
        {
            Left = center.X - size.X / 2,
            Right = center.X + size.X / 2,
            Top = center.Y - size.Y / 2,
            Bottom = center.Y + size.Y / 2
        };
    }

    public static RectD CreateAABB(VecD first, VecD second, VecD third, VecD fourth)
    {
        VecD min = new VecD(
            Math.Min(first.X, Math.Min(second.X, Math.Min(third.X, fourth.X))),
            Math.Min(first.Y, Math.Min(second.Y, Math.Min(third.Y, fourth.Y))));

        VecD max = new VecD(
            Math.Max(first.X, Math.Max(second.X, Math.Max(third.X, fourth.X))),
            Math.Max(first.Y, Math.Max(second.Y, Math.Max(third.Y, fourth.Y))));

        return new RectD(min, max - min);
    }

    public static RectD? FromPoints(Point[] points)
    {
        if (points.Length == 0)
            return null;

        double minX, minY, maxX, maxY;
        minY = double.MaxValue;
        minX = double.MaxValue;
        maxY = double.MinValue;
        maxX = double.MinValue;

        foreach (Point point in points)
        {
            if (point.X < minX)
                minX = point.X;
            if (point.X > maxX)
                maxX = point.X;
            if (point.Y < minY)
                minY = point.Y;
            if (point.Y > maxY)
                maxY = point.Y;
        }

        return FromTwoPoints(new VecD(minX, minY), new VecD(maxX, maxY));
    }

    /// <summary>
    /// Converts rectangles with negative dimensions into a normal one
    /// </summary>
    public readonly RectD Standardize()
    {
        (double newLeft, double newRight) = left > right ? (right, left) : (left, right);
        (double newTop, double newBottom) = top > bottom ? (bottom, top) : (top, bottom);
        return new RectD()
        {
            Left = newLeft,
            Right = newRight,
            Top = newTop,
            Bottom = newBottom
        };
    }

    public readonly RectD ReflectX(double verLineX)
    {
        return RectD.FromTwoPoints(Pos.ReflectX(verLineX), (Pos + Size).ReflectX(verLineX));
    }

    public readonly RectD ReflectY(double horLineY)
    {
        return RectD.FromTwoPoints(Pos.ReflectY(horLineY), (Pos + Size).ReflectY(horLineY));
    }

    public readonly RectD Offset(VecD offset) => Offset(offset.X, offset.Y);
    public readonly RectD Offset(double x, double y)
    {
        return new RectD()
        {
            Left = left + x,
            Right = right + x,
            Top = top + y,
            Bottom = bottom + y
        };
    }

    public readonly RectD Inflate(VecD amount) => Inflate(amount.Y, amount.Y);
    public readonly RectD Inflate(double x, double y)
    {
        return new RectD()
        {
            Left = left - x,
            Right = right + x,
            Top = top - y,
            Bottom = bottom + y,
        };
    }

    public readonly RectD Inflate(double amount)
    {
        return new RectD()
        {
            Left = left - amount,
            Right = right + amount,
            Top = top - amount,
            Bottom = bottom + amount,
        };
    }

    public readonly RectD Scale(double multiplier)
    {
        return new RectD()
        {
            Left = left * multiplier,
            Right = right * multiplier,
            Top = top * multiplier,
            Bottom = bottom * multiplier
        };
    }

    public readonly RectD Scale(double multiplier, VecD relativeTo)
    {
        return new RectD()
        {
            Left = (left - relativeTo.X) * multiplier + relativeTo.X,
            Right = (right - relativeTo.X) * multiplier + relativeTo.X,
            Top = (top - relativeTo.Y) * multiplier + relativeTo.Y,
            Bottom = (bottom - relativeTo.Y) * multiplier + relativeTo.Y
        };
    }

    public readonly RectD Translate(VecD delta)
    {
        return new RectD()
        {
            Left = left + delta.X,
            Right = right + delta.X,
            Top = top + delta.Y,
            Bottom = bottom + delta.Y
        };
    }

    /// <summary>
    /// Fits passed rectangle into this rectangle while maintaining aspect ratio
    /// </summary>
    public readonly RectD AspectFit(RectD rect)
    {
        double widthRatio = Width / rect.Width;
        double heightRatio = Height / rect.Height;
        if (widthRatio > heightRatio)
        {
            double newWidth = Height * rect.Width / rect.Height;
            double newLeft = left + (Width - newWidth) / 2;
            return new RectD(new(newLeft, top), new(newWidth, Height));
        }
        else
        {
            double newHeight = Width * rect.Height / rect.Width;
            double newTop = top + (Height - newHeight) / 2;
            return new RectD(new(left, newTop), new(Width, newHeight));
        }
    }

    public readonly RectD Round()
    {
        return new RectD()
        {
            Left = Math.Round(left),
            Right = Math.Round(right),
            Top = Math.Round(top),
            Bottom = Math.Round(bottom)
        };
    }

    public readonly RectD RoundOutwards()
    {
        return new RectD()
        {
            Left = Math.Floor(left),
            Right = Math.Ceiling(right),
            Top = Math.Floor(top),
            Bottom = Math.Ceiling(bottom)
        };
    }

    public readonly RectD RoundInwards()
    {
        return new RectD()
        {
            Left = Math.Ceiling(left),
            Right = Math.Floor(right),
            Top = Math.Ceiling(top),
            Bottom = Math.Floor(bottom)
        };
    }

    public readonly bool ContainsInclusive(VecD point) => ContainsInclusive(point.X, point.Y);
    public readonly bool ContainsInclusive(double x, double y)
    {
        return x >= left && x <= right && y >= top && y <= bottom;
    }

    public readonly bool ContainsExclusive(VecD point) => ContainsExclusive(point.X, point.Y);
    public readonly bool ContainsExclusive(double x, double y)
    {
        return x > left && x < right && y > top && y < bottom;
    }

    public readonly bool IntersectsWithInclusive(RectD rect)
    {
        return left <= rect.right && right >= rect.left && top <= rect.bottom && bottom >= rect.top;
    }

    public readonly bool IntersectsWithExclusive(RectD rect)
    {
        return left < rect.right && right > rect.left && top < rect.bottom && bottom > rect.top;
    }

    public readonly RectD Intersect(RectD other)
    {
        double left = Math.Max(this.left, other.left);
        double top = Math.Max(this.top, other.top);

        double right = Math.Min(this.right, other.right);
        double bottom = Math.Min(this.bottom, other.bottom);

        if (left >= right || top >= bottom)
            return RectD.Empty;

        return new RectD()
        {
            Left = left,
            Right = right,
            Top = top,
            Bottom = bottom
        };
    }

    public readonly RectD Union(RectD other)
    {
        double left = Math.Min(this.left, other.left);
        double top = Math.Min(this.top, other.top);

        double right = Math.Max(this.right, other.right);
        double bottom = Math.Max(this.bottom, other.bottom);

        if (left >= right || top >= bottom)
            return RectD.Empty;

        return new RectD()
        {
            Left = left,
            Right = right,
            Top = top,
            Bottom = bottom
        };
    }

    public static explicit operator RectI(RectD rect)
    {
        return new RectI()
        {
            Left = (int)rect.left,
            Right = (int)rect.right,
            Top = (int)rect.top,
            Bottom = (int)rect.bottom
        };
    }

    public static explicit operator RectD(RectI rect)
    {
        return new RectD()
        {
            Left = rect.Left,
            Right = rect.Right,
            Top = rect.Top,
            Bottom = rect.Bottom
        };
    }

    public static bool operator ==(RectD left, RectD right)
    {
        return left.left == right.left && left.right == right.right && left.top == right.top && left.bottom == right.bottom;
    }

    public static bool operator !=(RectD left, RectD right)
    {
        return !(left.left == right.left && left.right == right.right && left.top == right.top && left.bottom == right.bottom);
    }

    public readonly override bool Equals(object? obj)
    {
        return obj is RectD rect && rect.left == left && rect.right == right && rect.top == top && rect.bottom == bottom;
    }

    public readonly bool Equals(RectD other)
    {
        return left == other.left && top == other.top && right == other.right && bottom == other.bottom;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(left, top, right, bottom);
    }

    public override string ToString()
    {
        return $"{{X: {X}, Y: {Y}, W: {Width}, H: {Height}}}";
    }
}
