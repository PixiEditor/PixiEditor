using Avalonia;
using ChunkyImageLib.Operations;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using PixiEditor.Views.Rendering;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Views.Overlays.BrushShapeOverlay;
#nullable enable
internal class BrushShapeOverlay : Overlay
{
    public static readonly StyledProperty<float> BrushSizeProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, float>(nameof(BrushSize), defaultValue: 1);


    public static readonly StyledProperty<Scene> SceneProperty = AvaloniaProperty.Register<BrushShapeOverlay, Scene>(
        nameof(Scene));

    public static readonly StyledProperty<VectorPath?> BrushShapeProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, VectorPath?>(
            nameof(BrushShape));

    public VectorPath? BrushShape
    {
        get => GetValue(BrushShapeProperty);
        set => SetValue(BrushShapeProperty, value);
    }

    public Scene Scene
    {
        get => GetValue(SceneProperty);
        set => SetValue(SceneProperty, value);
    }

    public float BrushSize
    {
        get => (float)GetValue(BrushSizeProperty);
        set => SetValue(BrushSizeProperty, value);
    }

    private Paint paint = new Paint() { Color = Colors.LightGray, StrokeWidth = 1, Style = PaintStyle.Stroke };
    private VecD lastMousePos = new();

    private float lastSize;
    private VectorPath lastNonTranslatedCircle;


    static BrushShapeOverlay()
    {
        AffectsOverlayRender(BrushShapeProperty, BrushSizeProperty);
    }

    public BrushShapeOverlay()
    {
        IsHitTestVisible = false;
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        if (BrushShape == null)
            return;

        VecD rawPoint = args.Point;
        lastMousePos = rawPoint;
        Refresh();
    }

    protected override void OnRenderOverlay(Canvas context, RectD canvasBounds) => Render(context);

    public void Render(Canvas targetCanvas)
    {
        if (BrushShape != null)
        {
            paint.IsAntiAliased = true;
            targetCanvas.Save();
            /*using var path = new VectorPath(BrushShape);
            var rect = new RectD(lastMousePos - new VecD((BrushSize / 2f)), new VecD(BrushSize));

            path.Offset(rect.Center - path.Bounds.Center);

            VecD scale = new VecD(rect.Size.X / (float)path.Bounds.Width, rect.Size.Y / (float)path.Bounds.Height);
            if (scale.IsNaNOrInfinity())
            {
                scale = VecD.Zero;
            }

            VecD uniformScale = new VecD(Math.Min(scale.X, scale.Y));
            path.Transform(Matrix3X3.CreateScale((float)uniformScale.X, (float)uniformScale.Y, (float)rect.Center.X,
                (float)rect.Center.Y));

            */
            targetCanvas.DrawPath(BrushShape, paint);
            targetCanvas.Restore();
        }
    }

    protected override void ZoomChanged(double newZoom)
    {
        paint.StrokeWidth = (float)(1.0f / newZoom);
    }
}
