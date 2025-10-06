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

    public ColorBounds(ColorF color, double tolerance = 0)
    {
        float a = color.A;

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

    private static (float lower, float upper) FindInclusiveBoundaryPremul(float channel, float alpha)
    {
        var step = 1f / 255f;
        float subHalf = channel > 0 ? channel - step : channel;
        float addHalf = channel < 1 ? channel + step : channel;

        var lower = subHalf * alpha;
        var upper = addHalf * alpha;

        return (lower, upper);
    }

    private static (float lower, float upper) FindInclusiveBoundary(float channel)
    {
        float halfStep = 0.5f / 255f;
        float subHalf = channel > 0 ? channel - halfStep : channel;
        float addHalf = channel < 1 ? channel + halfStep : channel;
        return (subHalf, addHalf);
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

    public bool IsWithinBounds(ColorF toCompare)
    {
        float a = toCompare.A;
        float r = (toCompare.R) * a;
        float g = (toCompare.G) * a;
        float b = (toCompare.B) * a;
        
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

