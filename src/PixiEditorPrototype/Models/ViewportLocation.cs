using System;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.Models;
internal readonly record struct ViewportLocation(double Angle, Vector2d Center, Vector2d RealDimensions, Vector2d Dimensions, Guid GuidValue)
{
    public ChunkResolution Resolution
    {
        get
        {
            Vector2d densityVec = Dimensions.Divide(RealDimensions);
            double density = Math.Min(densityVec.X, densityVec.Y);
            if (density > 8.01)
                return ChunkResolution.Eighth;
            else if (density > 4.01)
                return ChunkResolution.Quarter;
            else if (density > 2.01)
                return ChunkResolution.Half;
            return ChunkResolution.Full;
        }
    }
}
