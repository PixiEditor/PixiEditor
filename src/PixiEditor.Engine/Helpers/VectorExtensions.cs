using PixiEditor.Numerics;
using Silk.NET.Maths;

namespace PixiEditor.Engine.Helpers;

public static class VectorExtensions
{
    public static VecI ToVecI(this Vector2D<int> vector)
    {
        return new VecI(vector.X, vector.Y);
    }
    
    public static Vector2D<int> ToVector2D(this VecI vector)
    {
        return new Vector2D<int>(vector.X, vector.Y);
    }
}
