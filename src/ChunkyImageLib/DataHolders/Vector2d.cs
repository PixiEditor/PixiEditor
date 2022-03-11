using SkiaSharp;

namespace ChunkyImageLib.DataHolders
{
    public struct Vector2d
    {
        public double X { set; get; }
        public double Y { set; get; }

        public double TaxicabLength => Math.Abs(X) + Math.Abs(Y);
        public double Length => Math.Sqrt(LengthSquared);
        public double LengthSquared => X * X + Y * Y;
        public double Angle => Y < 0 ? -AngleTo(new Vector2d(1, 0)) : AngleTo(new Vector2d(1, 0));

        public Vector2d(double x, double y)
        {
            X = x;
            Y = y;
        }
        public static Vector2d FromAngleAndLength(double angle, double length)
        {
            return new Vector2d(Math.Cos(angle) * length, Math.Sin(angle) * length);
        }

        public Vector2d Rotate(double angle)
        {
            Vector2d result = new Vector2d();
            result.X = X * Math.Cos(angle) - Y * Math.Sin(angle);
            result.Y = X * Math.Sin(angle) + Y * Math.Cos(angle);
            return result;
        }
        public double DistanceToLineSegment(Vector2d pos1, Vector2d pos2)
        {
            Vector2d segment = pos2 - pos1;
            if ((this - pos1).AngleTo(segment) > Math.PI / 2)
                return (this - pos1).Length;
            if ((this - pos2).AngleTo(-segment) > Math.PI / 2)
                return (this - pos2).Length;
            return DistanceToLine(pos1, pos2);
        }
        public double DistanceToLine(Vector2d pos1, Vector2d pos2)
        {
            double a = (pos1 - pos2).Length;
            double b = (this - pos1).Length;
            double c = (this - pos2).Length;

            double p = (a + b + c) / 2;
            double triangleArea = Math.Sqrt(p * (p - a) * (p - b) * (p - c));

            return triangleArea / a * 2;
        }
        public double AngleTo(Vector2d other)
        {
            return Math.Acos((this * other) / Length / other.Length);
        }

        public double CCWAngleTo(Vector2d other)
        {
            var rot = other.Rotate(-Angle);
            return rot.Angle;
        }

        public Vector2d Normalize()
        {
            return new Vector2d(X / Length, Y / Length);
        }
        public Vector2d Signs()
        {
            return new Vector2d(X >= 0 ? 1 : -1, Y >= 0 ? 1 : -1);
        }
        public Vector2d Multiply(Vector2d other)
        {
            return new Vector2d(X * other.X, Y * other.Y);
        }
        public static Vector2d operator +(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X + b.X, a.Y + b.Y);
        }
        public static Vector2d operator -(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.X - b.X, a.Y - b.Y);
        }
        public static Vector2d operator -(Vector2d a)
        {
            return new Vector2d(-a.X, -a.Y);
        }
        public static Vector2d operator *(double b, Vector2d a)
        {
            return new Vector2d(a.X * b, a.Y * b);
        }
        public static double operator *(Vector2d a, Vector2d b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
        public static Vector2d operator *(Vector2d a, double b)
        {
            return new Vector2d(a.X * b, a.Y * b);
        }
        public static Vector2d operator /(Vector2d a, double b)
        {
            return new Vector2d(a.X / b, a.Y / b);
        }
        public static bool operator ==(Vector2d a, Vector2d b)
        {
            return a.X == b.X && a.Y == b.Y;
        }
        public static bool operator !=(Vector2d a, Vector2d b)
        {
            return !(a.X == b.X && a.Y == b.Y);
        }

        public static explicit operator Vector2i(Vector2d vec)
        {
            return new Vector2i((int)vec.X, (int)vec.Y);
        }
        public static explicit operator SKPointI(Vector2d vec)
        {
            return new SKPointI((int)vec.X, (int)vec.Y);
        }
        public static explicit operator SKPoint(Vector2d vec)
        {
            return new SKPoint((float)vec.X, (float)vec.Y);
        }
        public static explicit operator SKSizeI(Vector2d vec)
        {
            return new SKSizeI((int)vec.X, (int)vec.Y);
        }
        public static explicit operator SKSize(Vector2d vec)
        {
            return new SKSize((float)vec.X, (float)vec.Y);
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public override bool Equals(object? obj)
        {
            var item = obj as Vector2d?;
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
