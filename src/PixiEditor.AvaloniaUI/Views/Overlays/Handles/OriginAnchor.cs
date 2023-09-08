using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.AvaloniaUI.Views.Overlays.TransformOverlay;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Views.Overlays.Handles;

public class OriginAnchor : Handle
{
    public IPen? SecondaryHandlePen { get; set; } = new Pen(Brushes.White, 1);

    public OriginAnchor(Control owner, VecD position) : base(owner, position)
    {

    }

    public override void Draw(DrawingContext context)
    {
        double radius = Size.LongestAxis / ZoomboxScale / 2;
        context.DrawEllipse(HandleBrush, HandlePen, TransformHelper.ToPoint(Position), radius, radius);
        context.DrawEllipse(HandleBrush, SecondaryHandlePen, TransformHelper.ToPoint(Position), radius, radius);
    }
}
