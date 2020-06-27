namespace PixiEditor.Models.Position
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

        public override string ToString()
        {
            return $"{X}, {Y}";
        }

        public static bool operator ==(Coordinates c1, Coordinates c2)
        {
            return c2.X == c1.X && c2.Y == c1.Y;
        }

        public static bool operator !=(Coordinates c1, Coordinates c2)
        {
            return !(c1 == c2);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Coordinates)) return false;
            return this == (Coordinates) obj;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int hashingBase = (int) 2166136261;
                const int hashingMultiplier = 16777619;

                int hash = hashingBase;
                hash = (hash * hashingMultiplier) ^ (!ReferenceEquals(null, X) ? X.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^ (!ReferenceEquals(null, Y) ? Y.GetHashCode() : 0);
                return hash;
            }
        }
    }
}