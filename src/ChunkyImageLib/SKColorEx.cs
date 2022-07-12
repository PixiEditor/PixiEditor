using SkiaSharp;

namespace ChunkyImageLib;
public static class SKColorEx
{
    public static unsafe ulong ToULong(this SKColor color)
    {
        ulong result = 0;
        Half* ptr = (Half*)&result;
        float normalizedAlpha = color.Alpha / 255.0f;
        ptr[0] = (Half)(color.Red / 255f * normalizedAlpha);
        ptr[1] = (Half)(color.Green / 255f * normalizedAlpha);
        ptr[2] = (Half)(color.Blue / 255f * normalizedAlpha);
        ptr[3] = (Half)(normalizedAlpha);
        return result;
    }
}
