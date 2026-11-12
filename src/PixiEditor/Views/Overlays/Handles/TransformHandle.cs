using Avalonia.Input;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.Views.Overlays.TransformOverlay;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.Handles;

public class TransformHandle : Handle
{
    public double AnchorRadius { get; set; } = GetResource<double>("AnchorRadius");
    public Paint GlyphPaint { get; set; } = GetPaint("HandleBrush");

    private HandleGlyph handleGlyph;

    public TransformHandle(Overlay owner) : base(owner)
    {
        handleGlyph = new IconGlyph(PixiPerfectIcons.MoveView, customPaint: GlyphPaint); 
        handleGlyph.Size = Size - new VecD(1);
        handleGlyph.Offset = new VecD(0, -1f);
        
        Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    protected override void OnDraw(Canvas context)
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
        
        handleGlyph.Draw(context, ZoomScale, Position);
    }
}
