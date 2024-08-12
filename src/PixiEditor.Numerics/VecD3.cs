namespace PixiEditor.Numerics;

public struct VecD3
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public VecD XY => new(X, Y);

    public double TaxicabLength => Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
    public double Length => Math.Sqrt(LengthSquared);
    public double LengthSquared => X * X + Y * Y + Z * Z;

    public static VecD3 Zero { get; } = new(0, 0, 0);

    public VecD3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public VecD3(VecD xy, double z) : this(xy.X, xy.Y, z)
    {
    }

    public VecD3(double bothAxesValue) : this(bothAxesValue, bothAxesValue, bothAxesValue)
    {
    }
    
    public VecD3 Round()
    {
        return new VecD3(Math.Round(X), Math.Round(Y), Math.Round(Z));
    }
    public VecD3 Ceiling()
    {
        return new VecD3(Math.Ceiling(X), Math.Ceiling(Y), Math.Ceiling(Z));
    }
    public VecD3 Floor()
    {
        return new VecD3(Math.Floor(X), Math.Floor(Y), Math.Floor(Z));
    }
    
    public VecD3 Lerp(VecD3 other, double factor)
    {
        return (other - this) * factor + this;
    }
    
    public VecD3 Normalize()
    {
        return new VecD3(X / Length, Y / Length, Z / Length);
    }
    
    public VecD3 Abs()
    {
        return new VecD3(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
    }
    
    public VecD3 Signs()
    {
        return new VecD3(X >= 0 ? 1 : -1, Y >= 0 ? 1 : -1, Z >= 0 ? 1 : -1);
    }
    
    public double Dot(VecD3 other) => (X * other.X) + (Y * other.Y) + (Z * other.Z);
    
    public VecD3 Multiply(VecD3 other)
    {
        return new VecD3(X * other.X, Y * other.Y, Z * other.Z);
    }
    
    public VecD3 Divide(VecD3 other)
    {
        return new VecD3(X / other.X, Y / other.Y, Z / other.Z);
    }
    
    public static VecD3 operator +(VecD3 a, VecD3 b)
    {
        return new VecD3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }
    
    public static VecD3 operator -(VecD3 a, VecD3 b)
    {
        return new VecD3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static VecD3 operator -(VecD3 a)
    {
        return new VecD3(-a.X, -a.Y, -a.Z);
    }
    
    public static VecD3 operator *(double b, VecD3 a)
    {
        return new VecD3(a.X * b, a.Y * b, a.Z * b);
    }

    public static double operator *(VecD3 a, VecD3 b) => a.Dot(b);
    
    public static VecD3 operator *(VecD3 a, double b)
    {
        return new VecD3(a.X * b, a.Y * b, a.Z * b);
    }
    
    public static VecD3 operator /(VecD3 a, double b)
    {
        return new VecD3(a.X / b, a.Y / b, a.Z / b);
    }
    
    public static VecD3 operator %(VecD3 a, double b)
    {
        return new VecD3(a.X % b, a.Y % b, a.Z % b);
    }

    public static bool operator ==(VecD3 a, VecD3 b)
    {
        return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    }
    public static bool operator !=(VecD3 a, VecD3 b)
    {
        return !(a.X == b.X && a.Y == b.Y && a.Z == b.Z);
    }

    public double Sum() => X + Y + Z;

    public static implicit operator VecD3((double, double, double, double) tuple)
    {
        return new VecD3(tuple.Item1, tuple.Item2, tuple.Item3);
    }
    
    public void Deconstruct(out double x, out double y, out double z)
    {
        x = X;
        y = Y;
        z = Z;
    }
    
    public bool IsNaNOrInfinity()
    {
        return double.IsNaN(X) || double.IsNaN(Y) || double.IsInfinity(X) || double.IsInfinity(Y) || double.IsNaN(Z) || double.IsInfinity(Z);
    }

    public override string ToString()
    {
        return $"({X}; {Y}; {Z})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not VecD3 item)
            return false;
        
        return this == (VecD3?)item;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public bool Equals(VecD3 other)
    {
        return other.X == X && other.Y == Y && other.Z == Z;
    }

    public bool AlmostEquals(VecD3 other, double axisEpsilon = 0.001)
    {
        double dX = Math.Abs(X - other.X);
        double dY = Math.Abs(Y - other.Y);
        double dZ = Math.Abs(Z - other.Z);
        return dX < axisEpsilon && dY < axisEpsilon && dZ < axisEpsilon;
    }
}
