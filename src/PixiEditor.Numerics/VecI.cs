namespace PixiEditor.Numerics;

public struct VecI : IEquatable<VecI>, IComparable<VecI>
{
    public int X { set; get; }
    public int Y { set; get; }

    public int TaxicabLength => Math.Abs(X) + Math.Abs(Y);
    public double Length => Math.Sqrt(LengthSquared);
    public int LengthSquared => X * X + Y * Y;
    public int LongestAxis => (Math.Abs(X) < Math.Abs(Y)) ? Y : X;
    public int ShortestAxis => (Math.Abs(X) < Math.Abs(Y)) ? X : Y;

    public static VecI Zero { get; } = new(0, 0);
    public static VecI One { get; } = new(1, 1);
    public static VecI NegativeOne { get; } = new(-1, -1);

    public VecI(int x, int y)
    {
        X = x;
        Y = y;
    }
    public VecI(int bothAxesValue)
    {
        X = bothAxesValue;
        Y = bothAxesValue;
    }

    /// <summary>
    ///     Returns a new vector with X and Y signed. Zero values are considered positive.
    /// </summary>
    /// <returns>Vector with X and Y values, where each value is 1 if original value is positive or 0, and -1 if original value is negative.</returns>
    public VecI Signs()
    {
        return new VecI(X >= 0 ? 1 : -1, Y >= 0 ? 1 : -1);
    }

    /// <summary>
    ///    Returns a new vector with X and Y signed. Zero values are signed as 0.
    /// </summary>
    /// <returns>Vector with X and Y values, where each value is 1 if original value is positive, -1 if original value is negative, and 0 if original value is 0.</returns>
    public VecI SignsWithZero()
    {
        return new VecI(Math.Sign(X), Math.Sign(Y));
    }

    public VecI Multiply(VecI other)
    {
        return new VecI(X * other.X, Y * other.Y);
    }
    public VecI Add(int value)
    {
        return new VecI(X + value, Y + value);
    }
    /// <summary>
    /// Reflects the vector across a vertical line with the specified x position
    /// </summary>
    public VecI ReflectX(int lineX)
    {
        return new(2 * lineX - X, Y);
    }
    /// <summary>
    /// Reflects the vector across a horizontal line with the specified y position
    /// </summary>
    public VecI ReflectY(int lineY)
    {
        return new(X, 2 * lineY - Y);
    }
    /// <summary>
    /// Reflects the vector across a vertical line with the specified x position
    /// </summary>
    public VecD ReflectX(double lineX)
    {
        return new(2 * lineX - X, Y);
    }
    /// <summary>
    /// Reflects the vector across a horizontal line with the specified y position
    /// </summary>
    public VecD ReflectY(double lineY)
    {
        return new(X, 2 * lineY - Y);
    }

    public VecI KeepInside(RectI rect)
    {
        return new VecI(Math.Clamp(X, rect.Left, rect.Right), Math.Clamp(Y, rect.Top, rect.Bottom));
    }

    public byte[] ToByteArray()
    {
        var data = new byte[sizeof(int) * 2];

        BitConverter.TryWriteBytes(data, X);
        BitConverter.TryWriteBytes(data.AsSpan(4), Y);

        return data;
    }

    public static VecI FromBytes(ReadOnlySpan<byte> value)
    {
        if (value.Length < sizeof(int) * 2)
        {
            throw new ArgumentException("Value bytes are invalid. Span must have 8 bytes", nameof(value));
        }

        var x = BitConverter.ToInt32(value);
        var y = BitConverter.ToInt32(value[4..]);

        return new VecI(x, y);
    }
    
    public static VecI operator +(VecI a, VecI b)
    {
        return new VecI(a.X + b.X, a.Y + b.Y);
    }
    public static VecI operator -(VecI a, VecI b)
    {
        return new VecI(a.X - b.X, a.Y - b.Y);
    }
    public static VecI operator -(VecI a)
    {
        return new VecI(-a.X, -a.Y);
    }
    public static VecI operator *(int b, VecI a)
    {
        return new VecI(a.X * b, a.Y * b);
    }
    public static int operator *(VecI a, VecI b)
    {
        return a.X * b.X + a.Y * b.Y;
    }
    public static VecI operator *(VecI a, int b)
    {
        return new VecI(a.X * b, a.Y * b);
    }
    public static VecD operator *(VecI a, double b)
    {
        return new VecD(a.X * b, a.Y * b);
    }
    public static VecI operator /(VecI a, int b)
    {
        return new VecI(a.X / b, a.Y / b);
    }
    public static VecD operator /(VecI a, double b)
    {
        return new VecD(a.X / b, a.Y / b);
    }
    public static VecI operator %(VecI a, int b)
    {
        return new(a.X % b, a.Y % b);
    }
    public static VecD operator %(VecI a, double b)
    {
        return new(a.X % b, a.Y % b);
    }
    public static bool operator ==(VecI a, VecI b)
    {
        return a.X == b.X && a.Y == b.Y;
    }
    public static bool operator !=(VecI a, VecI b)
    {
        return !(a.X == b.X && a.Y == b.Y);
    }
    public static implicit operator VecI(Point point)
    {
        return new VecI((int)point.X, (int)point.Y);
    }

    public static implicit operator Point(VecI vec)
    {
        return new(vec.X, vec.Y);
    }
    
    public static implicit operator VecD(VecI vec)
    {
        return new VecD(vec.X, vec.Y);
    }
    public static implicit operator VecI((int, int) tuple)
    {
        return new VecI(tuple.Item1, tuple.Item2);
    }

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }

    public override string ToString()
    {
        return $"({X}; {Y})";
    }

    public int CompareTo(VecI other)
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
        var item = obj as VecI?;
        if (item is null)
            return false;
        return this == item;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public bool Equals(VecI other)
    {
        return other.X == X && other.Y == Y;
    }

    public VecD Normalized()
    {
        return this / Length;
    }
}
