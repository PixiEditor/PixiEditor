using ComputeSharp;

namespace ChunkyImageLib.Shaders;

[AutoConstructor]
internal readonly partial struct ReplaceColorShader : IComputeShader
{
    public readonly ReadWriteTexture2D<UInt2> texture;
    public readonly HlslColorBounds colorBounds;
    public readonly UInt2 newColor;
    
    public void Execute()
    {
        UInt2 rgba = texture[ThreadIds.XY];
        Float4 rgbaFloat = ShaderUtils.UnpackPixel(rgba);
        
        if(IsWithinBounds(rgbaFloat))
        {
            texture[ThreadIds.XY] = newColor;
        }
    }

    private bool IsWithinBounds(Float4 color)
    {
        float r = color.R;
        float g = color.G;
        float b = color.B;
        float a = color.A;
        if (r < colorBounds.LowerR || r > colorBounds.UpperR)
            return false;
        if (g < colorBounds.LowerG || g > colorBounds.UpperG)
            return false;
        if (b < colorBounds.LowerB || b > colorBounds.UpperB)
            return false;
        return !(a < colorBounds.LowerA) && !(a > colorBounds.UpperA);
    }
}

public readonly struct HlslColorBounds
{
    public readonly float LowerR;
    public readonly float LowerG;
    public readonly float LowerB;
    public readonly float LowerA;
    public readonly float UpperR;
    public readonly float UpperG;
    public readonly float UpperB;
    public readonly float UpperA;

    public HlslColorBounds(Float4 color)
    {
        static (float lower, float upper) FindInclusiveBoundaryPremul(float channel, float alpha)
        {
            float subHalf = channel > 0 ? channel - .5f : channel;
            float addHalf = channel < 255 ? channel + .5f : channel;
            return (subHalf * alpha / 255f, addHalf * alpha / 255f);
        }

        static (float lower, float upper) FindInclusiveBoundary(float channel)
        {
            float subHalf = channel > 0 ? channel - .5f : channel;
            float addHalf = channel < 255 ? channel + .5f : channel;
            return (subHalf / 255f, addHalf / 255f);
        }

        float a = color.A / 255f;

        (LowerR, UpperR) = FindInclusiveBoundaryPremul(color.R, a);
        (LowerG, UpperG) = FindInclusiveBoundaryPremul(color.G, a);
        (LowerB, UpperB) = FindInclusiveBoundaryPremul(color.B, a);
        (LowerA, UpperA) = FindInclusiveBoundary(color.A);
    }
}
