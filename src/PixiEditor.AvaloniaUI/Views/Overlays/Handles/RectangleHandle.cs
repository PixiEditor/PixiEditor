using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.UI.Overlays;

namespace PixiEditor.AvaloniaUI.Views.Overlays.Handles;

public class RectangleHandle : Handle
{
    public RectangleHandle(Control owner, VecD position, VecD size) : base(owner, position, size)
    {
    }

    public override void Draw(DrawingContext context)
    {
        float scaleMultiplier = (float)(1.0 / ZoomboxScale);
        float radius = 2.5f * scaleMultiplier;
        context.DrawRectangle(HandleBrush, HandlePen, TransformHelper.ToHandleRect(Position, Size, ZoomboxScale), radius, radius);
    }
}
