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
    private Paint selectedPaint;
    
    public bool IsSelected { get; set; } = false;

    public AnchorHandle(Overlay owner) : base(owner)
    {
        Size = new VecD(GetResource<double>("AnchorHandleSize"));
        paint = GetPaint("HandleBrush");
        selectedPaint = GetPaint("SelectedHandleBrush");
        StrokePaint = paint;
    }

    public override void Draw(Canvas context)
    {
        paint.StrokeWidth = (float)(1.0 / ZoomScale);
        selectedPaint.StrokeWidth = (float)(2.5 / ZoomScale);
        
        StrokePaint = IsSelected ? selectedPaint : paint;
        StrokePaint.Style = PaintStyle.Stroke;
        base.Draw(context);
    }
}
