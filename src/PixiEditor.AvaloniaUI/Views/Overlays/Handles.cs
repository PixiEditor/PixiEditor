using Avalonia.Media;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Overlays;

public static class Handles
{
    //TODO: Contextual handles
    public static void DrawRectangleHandle(this DrawingContext context, IBrush brush, IPen pen, VecD point, double scale)
    {
        float scaleMultiplier = (float)(1.0 / scale);
        float radius = 2.5f * scaleMultiplier;
        context.DrawRectangle(brush, pen, TransformHelper.ToAnchorRect(point, scale), radius, radius);
    }
}
