using Drawie.Backend.Core.ColorsImpl;

namespace ChunkyImageLib;
public static class ColorEx
{
    public static unsafe ulong ToULong(this Color color)
    {
        ulong result = 0;
        Half* ptr = (Half*)&result;
        float normalizedAlpha = color.A / 255.0f;
        ptr[0] = (Half)(color.R / 255f * normalizedAlpha);
        ptr[1] = (Half)(color.G / 255f * normalizedAlpha);
        ptr[2] = (Half)(color.B / 255f * normalizedAlpha);
        ptr[3] = (Half)(normalizedAlpha);
        return result;
    }
}
