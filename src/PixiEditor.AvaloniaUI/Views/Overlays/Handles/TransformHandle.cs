using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.UI.Overlays;

namespace PixiEditor.AvaloniaUI.Views.Overlays.Handles;

public class TransformHandle : Handle
{
    public IBrush GlyphBrush { get; set; } = GetBrush("HandleGlyphBrush");

    private Geometry handleGeometry = GetHandleGeometry("MoveHandle");

    public TransformHandle(Control owner, VecD position, VecD size) : base(owner, position, size)
    {
    }

    public override void Draw(DrawingContext context)
    {
        context.DrawRectangle(HandleBrush, HandlePen, TransformHelper.ToHandleRect(Position, Size, ZoomboxScale));
        double crossSize = HandleRect.Size.X - 1;

        handleGeometry.Transform = new MatrixTransform(
            new Matrix(
                0, crossSize / ZoomboxScale,
                crossSize / ZoomboxScale, 0,
                Position.X - crossSize / (ZoomboxScale * 2), Position.Y - crossSize / (ZoomboxScale * 2))
        );
        context.DrawGeometry(GlyphBrush, null, handleGeometry);
    }
}
