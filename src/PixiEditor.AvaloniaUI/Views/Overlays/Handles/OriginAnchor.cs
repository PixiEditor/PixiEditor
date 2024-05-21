using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.Extensions.UI.Overlays;

namespace PixiEditor.AvaloniaUI.Views.Overlays.Handles;

public class OriginAnchor : Handle
{
    public IPen? SecondaryHandlePen { get; set; } = new Pen(Brushes.White, 1);

    public OriginAnchor(Overlay owner) : base(owner)
    {

    }

    public override void Draw(DrawingContext context)
    {
        double radius = Size.LongestAxis / ZoomScale / 2;
        context.DrawEllipse(HandleBrush, HandlePen, TransformHelper.ToPoint(Position), radius, radius);
        context.DrawEllipse(HandleBrush, SecondaryHandlePen, TransformHelper.ToPoint(Position), radius, radius);
    }
}
