using System;
using System.Windows;

namespace PixiEditor.Helpers.Extensions
{
    static class Int32RectEx
    {
        public static Int32Rect Intersect(this Int32Rect rect, Int32Rect other)
        {
            int rectX2 = rect.X + rect.Width;
            int rectY2 = rect.Y + rect.Height;

            int otherX2 = other.X + other.Width;
            int otherY2 = other.Y + other.Height;

            int maxX1 = Math.Max(rect.X, other.X);
            int maxY1 = Math.Max(rect.Y, other.Y);

            int minX2 = Math.Min(rectX2, otherX2);
            int minY2 = Math.Min(rectY2, otherY2);

            int width = minX2 - maxX1;
            int height = minY2 - maxY1;

            if (width <= 0 || height <= 0)
                return Int32Rect.Empty;

            return new Int32Rect(maxX1, maxY1, width, height);
        }

        public static Int32Rect Expand(this Int32Rect rect, Int32Rect other)
        {
            int rectX2 = rect.X + rect.Width;
            int rectY2 = rect.Y + rect.Height;

            int otherX2 = other.X + other.Width;
            int otherY2 = other.Y + other.Height;

            int minX1 = Math.Min(rect.X, other.X);
            int minY1 = Math.Min(rect.Y, other.Y);

            int maxX2 = Math.Max(rectX2, otherX2);
            int maxY2 = Math.Max(rectY2, otherY2);

            int width = maxX2 - minX1;
            int height = maxY2 - minY1;

            if (width <= 0 || height <= 0)
                return Int32Rect.Empty;

            return new Int32Rect(minX1, minY1, width, height);
        }
    }
}
