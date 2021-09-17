using System.Windows;

namespace PixiEditor.Helpers.Extensions
{
    public static class Int32RectHelpers
    {
        public static Int32Rect Min(this Int32Rect rect, Int32Rect other)
        {
            int width = rect.Width;
            int height = rect.Height;

            if (width + rect.X > other.Width + other.X)
            {
                width = other.Width;
            }

            if (height + rect.Y > other.Height + other.Y)
            {
                height = other.Height;
            }

            return new Int32Rect(rect.X, rect.Y, width, height);
        }
    }
}
