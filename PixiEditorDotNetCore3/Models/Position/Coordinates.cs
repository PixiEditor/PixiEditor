using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditorDotNetCore3.Models.Position
{
    public struct Coordinates
    {
        public int X { get; set; }

        public int Y { get; set; }

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"x: {X}, y: {Y}";

        public static bool operator ==(Coordinates c1, Coordinates c2)
        {
            if (c1 == null || c2 == null) return false;
            return c2.X == c1.X && c2.Y == c1.Y;
        }

        public static bool operator !=(Coordinates c1, Coordinates c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Coordinates)) return false;
            return this == (Coordinates)obj;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int HashingBase = (int)2166136261;
                const int HashingMultiplier = 16777619;

                int hash = HashingBase;
                hash = (hash * HashingMultiplier) ^ (!ReferenceEquals(null, X) ? X.GetHashCode() : 0);
                hash = (hash * HashingMultiplier) ^ (!ReferenceEquals(null, Y) ? Y.GetHashCode() : 0);            
                return hash;
            }
        }
    }
}
