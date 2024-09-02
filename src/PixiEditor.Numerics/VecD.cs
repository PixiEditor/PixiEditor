namespace PixiEditor.Numerics;

public struct VecD : IEquatable<VecD>, IComparable<VecD>
{
    public double X { set; get; }
    public double Y { set; get; }

    public double TaxicabLength => Math.Abs(X) + Math.Abs(Y);
    public double Length => Math.Sqrt(LengthSquared);
    public double LengthSquared => X * X + Y * Y;
    public double Angle => Y < 0 ? -AngleTo(new VecD(1, 0)) : AngleTo(new VecD(1, 0));
    public double LongestAxis => (Math.Abs(X) < Math.Abs(Y)) ? Y : X;
    public double ShortestAxis => (Math.Abs(X) < Math.Abs(Y)) ? X : Y;

    public static VecD Zero { get; } = new(0, 0);

    public VecD(double x, double y)
    {
        X = x;
        Y = y;
    }

    public VecD(double bothAxesValue)
    {
        X = bothAxesValue;
        Y = bothAxesValue;
    }

    public static VecD FromAngleAndLength(double angle, double length)
    {
        return new VecD(Math.Cos(angle) * length, Math.Sin(angle) * length);
    }

    public VecD Round()
    {
        return new(Math.Round(X), Math.Round(Y));
    }

    public VecD Ceiling()
    {
        return new(Math.Ceiling(X), Math.Ceiling(Y));
    }

    public VecD Floor()
    {
        return new(Math.Floor(X), Math.Floor(Y));
    }

    /// <summary>
    ///     Rotates the vector by the specified angle in radians
    /// </summary>
    /// <param name="angleRad">Angle in radians</param>
    /// <returns>Rotated vector</returns>
    public VecD Rotate(double angleRad)
    {
        VecD result = new VecD();
        result.X = X * Math.Cos(angleRad) - Y * Math.Sin(angleRad);
        result.Y = X * Math.Sin(angleRad) + Y * Math.Cos(angleRad);
        return result;
    }

    public VecD Rotate(double angleRad, VecD around)
    {
        return (this - around).Rotate(angleRad) + around;
    }

    public double DistanceToLineSegment(VecD pos1, VecD pos2)
    {
        VecD segment = pos2 - pos1;
        if ((this - pos1).AngleTo(segment) > Math.PI / 2)
            return (this - pos1).Length;
        if ((this - pos2).AngleTo(-segment) > Math.PI / 2)
            return (this - pos2).Length;
        return DistanceToLine(pos1, pos2);
    }

    public double DistanceToLine(VecD pos1, VecD pos2)
    {
        double a = (pos1 - pos2).Length;
        double b = (this - pos1).Length;
        double c = (this - pos2).Length;

        double p = (a + b + c) / 2;
        double triangleArea = Math.Sqrt(p * (p - a) * (p - b) * (p - c));

        return triangleArea / a * 2;
    }

    public VecD ProjectOntoLine(VecD pos1, VecD pos2)
    {
        VecD line = (pos2 - pos1).Normalize();
        VecD point = this - pos1;
        return (line * point) * line + pos1;
    }

    /// <summary>
    /// Reflects the vector across a vertical line with the specified position
    /// </summary>
    public VecD ReflectX(double lineX)
    {
        return new(2 * lineX - X, Y);
    }

    /// <summary>
    /// Reflects the vector along a horizontal line with the specified position
    /// </summary>
    public VecD ReflectY(double lineY)
    {
        return new(X, 2 * lineY - Y);
    }

    public VecD ReflectAcrossLine(VecD pos1, VecD pos2)
    {
        var onLine = ProjectOntoLine(pos1, pos2);
        return onLine - (this - onLine);
    }

    public double AngleTo(VecD other)
    {
        return Math.Acos((this * other) / Length / other.Length);
    }

    /// <summary>
    /// Returns the angle between two vectors when travelling counterclockwise (assuming Y pointing up) from this vector to passed vector
    /// </summary>
    public double CCWAngleTo(VecD other)
    {
        var rot = other.Rotate(-Angle);
        return rot.Angle;
    }

    public VecD Lerp(VecD other, double factor)
    {
        return (other - this) * factor + this;
    }

    public VecD Normalize()
    {
        return new VecD(X / Length, Y / Length);
    }

    public VecD Abs()
    {
        return new VecD(Math.Abs(X), Math.Abs(Y));
    }

    public VecD Signs()
    {
        return new VecD(X >= 0 ? 1 : -1, Y >= 0 ? 1 : -1);
    }

    /// <summary>
    /// Returns the signed magnitude (Z coordinate) of the vector resulting from the cross product
    /// </summary>
    public double Cross(VecD other)
    {
        return (X * other.Y) - (Y * other.X);
    }

    public double Dot(VecD other) => (X * other.X) + (Y * other.Y);

    public VecD Multiply(VecD other)
    {
        return new VecD(X * other.X, Y * other.Y);
    }

    public VecD Divide(VecD other)
    {
        return new VecD(X / other.X, Y / other.Y);
    }

    public static VecD operator +(VecD a, VecD b)
    {
        return new VecD(a.X + b.X, a.Y + b.Y);
    }

    public static VecD operator -(VecD a, VecD b)
    {
        return new VecD(a.X - b.X, a.Y - b.Y);
    }

    public static VecD operator -(VecD a)
    {
        return new VecD(-a.X, -a.Y);
    }

    public static VecD operator *(double b, VecD a)
    {
        return new VecD(a.X * b, a.Y * b);
    }

    public static double operator *(VecD a, VecD b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    public static VecD operator *(VecD a, double b)
    {
        return new VecD(a.X * b, a.Y * b);
    }

    public static VecD operator /(VecD a, double b)
    {
        return new VecD(a.X / b, a.Y / b);
    }

    public static VecD operator %(VecD a, double b)
    {
        return new(a.X % b, a.Y % b);
    }

    public static bool operator ==(VecD a, VecD b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(VecD a, VecD b)
    {
        return !(a.X == b.X && a.Y == b.Y);
    }

    public static explicit operator VecI(VecD vec)
    {
        return new VecI((int)vec.X, (int)vec.Y);
    }

    public static implicit operator VecD((double, double) tuple)
    {
        return new VecD(tuple.Item1, tuple.Item2);
    }

    public void Deconstruct(out double x, out double y)
    {
        x = X;
        y = Y;
    }

    public bool IsNaNOrInfinity()
    {
        return double.IsNaN(X) || double.IsNaN(Y) || double.IsInfinity(X) || double.IsInfinity(Y);
    }

    public override string ToString()
    {
        return $"({X}; {Y})";
    }

    public int CompareTo(VecD other)
    {
        int xComparison = X.CompareTo(other.X);
        int yComparison = Y.CompareTo(other.Y);
        if (xComparison == 0 && yComparison == 0)
            return 0;

        bool anyNegative = xComparison < 0 || yComparison < 0;
        return anyNegative ? -1 : 1;
    }

    public override bool Equals(object? obj)
    {
        var item = obj as VecD?;
        if (item is null)
            return false;
        return this == item;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public bool Equals(VecD other)
    {
        return other.X == X && other.Y == Y;
    }

    public bool AlmostEquals(VecD other, double axisEpsilon = 0.001)
    {
        double dX = Math.Abs(X - other.X);
        double dY = Math.Abs(Y - other.Y);
        return dX < axisEpsilon && dY < axisEpsilon;
    }
}
