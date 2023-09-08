using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Overlays.Handles;

public class TransformHandle : Handle
{
    public double AnchorRadius { get; set; } = GetResource<double>("AnchorRadius");
    public IBrush GlyphBrush { get; set; } = GetBrush("HandleGlyphBrush");

    private Geometry handleGeometry = GetHandleGeometry("MoveHandle");

    public TransformHandle(Control owner) : base(owner)
    {
        Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    public override void Draw(DrawingContext context)
    {
        double scaleMultiplier = (1.0 / ZoomboxScale);
        double radius = AnchorRadius * scaleMultiplier;

        context.DrawRectangle(HandleBrush, HandlePen, TransformHelper.ToHandleRect(Position, Size, ZoomboxScale), radius, radius);
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
