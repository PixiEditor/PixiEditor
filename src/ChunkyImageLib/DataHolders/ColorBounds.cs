using System.Runtime.CompilerServices;
using Drawie.Backend.Core.ColorsImpl;

namespace ChunkyImageLib.DataHolders;

public struct ColorBounds
{
    public float LowerR { get; set; }
    
    public float LowerG { get; set; }
    
    public float LowerB { get; set; }
    
    public float LowerA { get; set; }
    
    public float UpperR { get; set; }
    
    public float UpperG { get; set; }
    
    public float UpperB { get; set; }

    public float UpperA { get; set; }

    public ColorBounds(Color color, double tolerance = 0)
    {
        static (float lower, float upper) FindInclusiveBoundaryPremul(byte channel, float alpha)
        {
            float subHalf = channel > 0 ? channel - 1f : channel;
            float addHalf = channel < 255 ? channel + 1f : channel;
            
            var lower = subHalf * alpha / 255f;
            var upper = addHalf * alpha / 255f;
            
            return (lower, upper);
        }

        static (float lower, float upper) FindInclusiveBoundary(byte channel)
        {
            float subHalf = channel > 0 ? channel - .5f : channel;
            float addHalf = channel < 255 ? channel + .5f : channel;
            return (subHalf / 255f, addHalf / 255f);
        }

        float a = color.A / 255f;

        (LowerR, UpperR) = FindInclusiveBoundaryPremul(color.R, a);
        LowerR -= (float)tolerance;
        UpperR += (float)tolerance;
        
        (LowerG, UpperG) = FindInclusiveBoundaryPremul(color.G, a);
        LowerG -= (float)tolerance;
        UpperG += (float)tolerance;
        
        (LowerB, UpperB) = FindInclusiveBoundaryPremul(color.B, a);
        LowerB -= (float)tolerance;
        UpperB += (float)tolerance;
        
        (LowerA, UpperA) = FindInclusiveBoundary(color.A);
        LowerA -= (float)tolerance;
        UpperA += (float)tolerance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe bool IsWithinBounds(Half* pixel)
    {
        float r = (float)pixel[0];
        float g = (float)pixel[1];
        float b = (float)pixel[2];
        float a = (float)pixel[3];
        if (r < LowerR || r > UpperR)
            return false;
        if (g < LowerG || g > UpperG)
            return false;
        if (b < LowerB || b > UpperB)
            return false;
        if (a < LowerA || a > UpperA)
            return false;
        return true;
    }

    public bool IsWithinBounds(Color toCompare)
    {
        float a = toCompare.A / 255f;
        float r = (toCompare.R / 255f) * a;
        float g = (toCompare.G / 255f) * a;
        float b = (toCompare.B / 255f) * a;
        
        if (r < LowerR || r > UpperR)
            return false;
        if (g < LowerG || g > UpperG)
            return false;
        if (b < LowerB || b > UpperB)
            return false;
        if (a < LowerA || a > UpperA)
            return false;
        return true;
    }
}

