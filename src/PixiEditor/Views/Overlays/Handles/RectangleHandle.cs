using Avalonia.Controls;
using Avalonia.Media;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Views.Overlays.TransformOverlay;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.Handles;

public class RectangleHandle : Handle
{
    public double AnchorRadius { get; set; } = GetResource<double>("AnchorRadius");
    public RectangleHandle(Overlay owner) : base(owner)
    {
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
    }
}
