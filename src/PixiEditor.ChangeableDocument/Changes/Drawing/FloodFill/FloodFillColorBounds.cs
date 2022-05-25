using SkiaSharp;

namespace PixiEditor.ChangeableDocument.Changes.Drawing.FloodFill;
internal struct FloodFillColorBounds
{
    public float LowerR { get; set; }
    public float LowerG { get; set; }
    public float LowerB { get; set; }
    public float LowerA { get; set; }
    public float UpperR { get; set; }
    public float UpperG { get; set; }
    public float UpperB { get; set; }
    public float UpperA { get; set; }

    public FloodFillColorBounds(SKColor color)
    {
        static (float lower, float upper) FindInclusiveBoundaryPremul(byte channel, float alpha)
        {
            float subHalf = channel > 0 ? channel - .5f : channel;
            float addHalf = channel < 255 ? channel + .5f : channel;
            return (subHalf * alpha / 255f, addHalf * alpha / 255f);
        }

        static (float lower, float upper) FindInclusiveBoundary(byte channel)
        {
            float subHalf = channel > 0 ? channel - .5f : channel;
            float addHalf = channel < 255 ? channel + .5f : channel;
            return (subHalf / 255f, addHalf / 255f);
        }

        float a = color.Alpha / 255f;

        (LowerR, UpperR) = FindInclusiveBoundaryPremul(color.Red, a);
        (LowerG, UpperG) = FindInclusiveBoundaryPremul(color.Green, a);
        (LowerB, UpperB) = FindInclusiveBoundaryPremul(color.Blue, a);
        (LowerA, UpperA) = FindInclusiveBoundary(color.Alpha);
    }
}
