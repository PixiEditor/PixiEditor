using SkiaSharp;

namespace ChunkyImageLib.DataHolders
{
    public struct Vector2i
    {
        public int X { set; get; }
        public int Y { set; get; }

        public int TaxicabLength => Math.Abs(X) + Math.Abs(Y);
        public double Length => Math.Sqrt(LengthSquared);
        public int LengthSquared => X * X + Y * Y;

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2i Signs()
        {
            return new Vector2i(X >= 0 ? 1 : -1, Y >= 0 ? 1 : -1);
        }
        public Vector2i Multiply(Vector2i other)
        {
            return new Vector2i(X * other.X, Y * other.Y);
        }
        public static Vector2i operator +(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.X + b.X, a.Y + b.Y);
        }
        public static Vector2i operator -(Vector2i a, Vector2i b)
        {
            return new Vector2i(a.X - b.X, a.Y - b.Y);
        }
        public static Vector2i operator -(Vector2i a)
        {
            return new Vector2i(-a.X, -a.Y);
        }
        public static Vector2i operator *(int b, Vector2i a)
        {
            return new Vector2i(a.X * b, a.Y * b);
        }
        public static int operator *(Vector2i a, Vector2i b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
        public static Vector2i operator *(Vector2i a, int b)
        {
            return new Vector2i(a.X * b, a.Y * b);
        }
        public static bool operator ==(Vector2i a, Vector2i b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(Vector2i a, Vector2i b)
        {
            return !(a.X == b.X && a.Y == b.Y);
        }

        public static implicit operator Vector2d(Vector2i vec)
        {
            return new Vector2d(vec.X, vec.Y);
        }
        public static implicit operator SKPointI(Vector2i vec)
        {
            return new SKPointI(vec.X, vec.Y);
        }
        public static implicit operator SKPoint(Vector2i vec)
        {
            return new SKPoint(vec.X, vec.Y);
        }
        public static implicit operator SKSizeI(Vector2i vec)
        {
            return new SKSizeI(vec.X, vec.Y);
        }
        public static implicit operator SKSize(Vector2i vec)
        {
            return new SKSize(vec.X, vec.Y);
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public override bool Equals(object? obj)
        {
            var item = obj as Vector2i?;
            if (item == null)
                return false;
            return this == item;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
