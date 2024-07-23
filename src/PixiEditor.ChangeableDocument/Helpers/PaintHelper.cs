using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace PixiEditor.ChangeableDocument.Helpers;

internal static class PaintHelper
{
    public static void SetFilters(this Paint paint, Filter? filter)
    {
        paint.ColorFilter = filter?.ColorFilter;
        paint.ImageFilter = filter?.ImageFilter;
    }
}
