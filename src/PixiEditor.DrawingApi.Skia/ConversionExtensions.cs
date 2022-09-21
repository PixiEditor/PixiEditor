using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia
{
    public static class ConversionExtensions
    {
        public static SKRect ToSKRect(this RectD rectD)
        {
            return SKRect.Create((float)rectD.X, (float)rectD.Y, (float)rectD.Width, (float)rectD.Height);
        }

        public static SKColor ToSKColor(this Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }
    }
}
