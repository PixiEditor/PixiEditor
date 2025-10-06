using Avalonia.Controls;
using Avalonia.Media;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Views.Overlays.TransformOverlay;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Views.Overlays.Handles;

public class OriginAnchor : Handle
{
    public Paint SecondaryHandlePen { get; set; } = new Paint() { Color = Colors.White, StrokeWidth = 1 };
    
    public OriginAnchor(Overlay owner) : base(owner)
    {

    }

    protected override void OnDraw(Canvas context)
    {
        double radius = Size.LongestAxis / ZoomScale / 2;
        
        context.DrawOval(Position, new VecD(radius), StrokePaint);
        context.DrawOval(Position, new VecD(radius), SecondaryHandlePen);
    }
}
