using PixiEditor.Parser;
using SkiaSharp;

namespace PixiEditor.Helpers.Extensions
{
    public static class PixiParserHelper
    {
        public static SKRectI GetRect(this SerializableLayer layer) =>
            SKRectI.Create(layer.OffsetX, layer.OffsetY, layer.Width, layer.Height);
    }
}
