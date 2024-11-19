using Avalonia.Controls;
using Avalonia.Media;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;

namespace PixiEditor.Views.Overlays.Handles;

public class AnchorHandle : RectangleHandle
{
    private Paint paint;
    public AnchorHandle(Overlay owner) : base(owner)
    {
        Size = new VecD(GetResource<double>("AnchorHandleSize"));
        paint = GetPaint("HandleBrush");
        StrokePaint = paint;
        StrokePaint.Style = PaintStyle.Stroke;
    }


    public override void Draw(Canvas context)
    {
        paint.StrokeWidth = (float)(1.0 / ZoomScale);
        base.Draw(context);
    }
}
