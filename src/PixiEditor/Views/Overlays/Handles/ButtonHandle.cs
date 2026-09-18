using System.Windows.Input;
using Avalonia.Input;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.Views.Overlays.TransformOverlay;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.Handles;

public class ButtonHandle : Handle
{
    public string Icon { get; set; }
    public double AnchorRadius { get; set; } = GetResource<double>("AnchorRadius");

    private Paint fontPaint = new() { Color = Colors.White, IsAntiAliased = true };

    private IconGlyph iconGlyph;
    private ICommand onPressed;

    public ButtonHandle(IOverlay owner, ICommand onPressed) : base(owner)
    {
        this.onPressed = onPressed;
        iconGlyph = new IconGlyph(Icon);
        StrokePaint = new Paint() { Color = Colors.Black, IsAntiAliased = true };
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    protected override void OnDraw(Canvas context)
    {
        double scaleMultiplier = (1.0 / ZoomScale);
        double radius = AnchorRadius * scaleMultiplier;
        RectD handleRect = TransformHelper.ToHandleRect(Position, Size, ZoomScale);
        context.DrawRoundRect((float)handleRect.X, (float)handleRect.Y, (float)handleRect.Width,
            (float)handleRect.Height,
            (float)radius, (float)radius, FillPaint);
        if (StrokePaint != null)
        {
            context.DrawRoundRect((float)handleRect.X, (float)handleRect.Y, (float)handleRect.Width,
                (float)handleRect.Height,
                (float)radius, (float)radius, StrokePaint);
        }

        iconGlyph.Icon = Icon;
        iconGlyph.Size = Size - new VecD(1);
        iconGlyph.Offset = new VecD(10, 9);
        iconGlyph.Draw(context, ZoomScale, handleRect.Pos);
    }

    public override void OnPressed(OverlayPointerArgs args)
    {
        if (onPressed.CanExecute(null))
        {
            onPressed.Execute(null);
        }
    }
}
