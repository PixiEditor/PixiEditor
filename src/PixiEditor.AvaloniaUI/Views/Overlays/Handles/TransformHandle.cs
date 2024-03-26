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

    private HandleGlyph handleGeometry;

    public TransformHandle(Control owner) : base(owner)
    {
        handleGeometry = GetHandleGlyph("MoveHandle");
        handleGeometry.Size = Size - new VecD(1, 1);

        Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    public override void Draw(DrawingContext context)
    {
        double scaleMultiplier = (1.0 / ZoomboxScale);
        double radius = AnchorRadius * scaleMultiplier;

        context.DrawRectangle(HandleBrush, HandlePen, TransformHelper.ToHandleRect(Position, Size, ZoomboxScale), radius, radius);
        handleGeometry.Draw(context, ZoomboxScale, Position);
    }
}
