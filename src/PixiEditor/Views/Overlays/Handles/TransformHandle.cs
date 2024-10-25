using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using PixiEditor.Views.Overlays.TransformOverlay;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.Handles;

public class TransformHandle : Handle
{
    public double AnchorRadius { get; set; } = GetResource<double>("AnchorRadius");
    public Paint GlyphPaint { get; set; } = GetPaint("HandleGlyphBrush");

    private HandleGlyph handleGeometry;

    public TransformHandle(Overlay owner) : base(owner)
    {
        handleGeometry = GetHandleGlyph("MoveHandle");
        handleGeometry.Size = Size - new VecD(1, 1);

        Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    public override void Draw(Canvas context)
    {
        double scaleMultiplier = (1.0 / ZoomScale);
        double radius = AnchorRadius * scaleMultiplier;

        RectD handleRect = TransformHelper.ToHandleRect(Position, Size, ZoomScale);
        context.DrawRoundRect((float)handleRect.X, (float)handleRect.Y, (float)handleRect.Width, (float)handleRect.Height,
            (float)radius, (float)radius, FillPaint);

        if (StrokePaint != null)
        {
            context.DrawRoundRect((float)handleRect.X, (float)handleRect.Y, (float)handleRect.Width,
                (float)handleRect.Height,
                (float)radius, (float)radius, StrokePaint);
        }
        
        handleGeometry.Draw(context, ZoomScale, Position);
    }
}
