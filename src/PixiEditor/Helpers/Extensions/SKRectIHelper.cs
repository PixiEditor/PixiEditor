using System.Windows;
using SkiaSharp;

namespace PixiEditor.Helpers.Extensions;

internal static class SKRectIHelper
{
    public static Int32Rect ToInt32Rect(this SKRectI rect)
    {
        return new Int32Rect(rect.Left, rect.Top, rect.Width, rect.Height);
    }
}
