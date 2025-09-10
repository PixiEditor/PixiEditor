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

    private VectorPath threePixelCircle;
    private float lastSize;
    private VectorPath lastNonTranslatedCircle;


    static BrushShapeOverlay()
    {
        AffectsOverlayRender(BrushShapeProperty, BrushSizeProperty);
    }

    public BrushShapeOverlay()
    {
        IsHitTestVisible = false;
        threePixelCircle = EllipseHelper.CreateThreePixelCircle(VecI.Zero);
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        if (BrushShape == null)
            return;

        VecD rawPoint = args.Point;
        lastMousePos = rawPoint;
        Refresh();
    }

    public override void RenderOverlay(Canvas context, RectD canvasBounds) => Render(context);

    public void Render(Canvas targetCanvas)
    {
        var winRect = new RectD(
            (VecD)(new VecD(Math.Floor(lastMousePos.X), Math.Floor(lastMousePos.Y)) -
                   new VecD(BrushSize / 2, BrushSize / 2)),
            new VecD(BrushSize, BrushSize));
        /*switch (BrushShape)
        {
            case BrushShape.Pixel:
                paint.IsAntiAliased = false;
                targetCanvas.DrawRect(
                    new RectD(new VecD(Math.Floor(lastMousePos.X), Math.Floor(lastMousePos.Y)), new VecD(1, 1)),
                    paint);
                break;
            case BrushShape.Square:
                targetCanvas.DrawRect(winRect, paint);
                break;
            case BrushShape.CirclePixelated:
                DrawCircleBrushShape(targetCanvas, winRect);
                break;
            case BrushShape.CircleSmooth:
                DrawCircleBrushShapeSmooth(targetCanvas, lastMousePos, BrushSize / 2f);
                break;
        }*/

        if (BrushShape != null)
        {
            paint.IsAntiAliased = true;
            targetCanvas.Save();
            using var path = new VectorPath(BrushShape);
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

            /*
            if (brushNode.FitToStrokeSize.Value)
            {
                VecD scale = new VecD(rect.Size.X / (float)path.Bounds.Width, rect.Size.Y / (float)path.Bounds.Height);
                if (scale.IsNaNOrInfinity())
                {
                    scale = VecD.Zero;
                }

                VecD uniformScale = new VecD(Math.Min(scale.X, scale.Y));
                path.Transform(Matrix3X3.CreateScale((float)uniformScale.X, (float)uniformScale.Y, (float)rect.Center.X,
                    (float)rect.Center.Y));
            }

            var pressure = brushNode.Pressure.Value;
            Matrix3X3 pressureScale = Matrix3X3.CreateScale(pressure, pressure, (float)rect.Center.X,
                (float)rect.Center.Y);
            path.Transform(pressureScale);
            */
            targetCanvas.DrawPath(path, paint);
            targetCanvas.Restore();
        }
    }

    private void DrawCircleBrushShape(Canvas drawingContext, RectD winRect)
    {
        paint.IsAntiAliased = false;

        var rectI = new RectI((int)winRect.X, (int)winRect.Y, (int)winRect.Width, (int)winRect.Height);
        if (BrushSize < 3)
        {
            drawingContext.DrawRect(winRect, paint);
        }
        else if (BrushSize == 3)
        {
            var lp = new VecI((int)lastMousePos.X, (int)lastMousePos.Y);
            using VectorPath shifted = new VectorPath(threePixelCircle);
            shifted.Transform(Matrix3X3.CreateTranslation(lp.X, lp.Y));
            drawingContext.DrawPath(shifted, paint);
        }
        else if (BrushSize > 200)
        {
            VecD center = rectI.Center;
            drawingContext.DrawOval(new VecD(center.X, center.Y), new VecD(rectI.Width / 2.0, rectI.Height / 2.0),
                paint);
        }
        else
        {
            if (BrushSize != lastSize)
            {
                var geometry = EllipseHelper.ConstructEllipseOutline(new RectI(0, 0, rectI.Width, rectI.Height));
                lastNonTranslatedCircle = new VectorPath(geometry);
                lastSize = BrushSize;
            }

            var lp = new VecI((int)lastMousePos.X, (int)lastMousePos.Y);
            using VectorPath shifted = new VectorPath(lastNonTranslatedCircle);
            shifted.Transform(Matrix3X3.CreateTranslation(lp.X - rectI.Width / 2,
                lp.Y - rectI.Height / 2)); // don't use float, truncation is intended 
            drawingContext.DrawPath(shifted, paint);
        }
    }

    private void DrawCircleBrushShapeSmooth(Canvas drawingContext, VecD lastMousePos, float radius)
    {
        VecD center = lastMousePos;
        paint.IsAntiAliased = true;

        drawingContext.DrawOval(new VecD(center.X, center.Y), new VecD(radius, radius),
            paint);
    }

    protected override void ZoomChanged(double newZoom)
    {
        paint.StrokeWidth = (float)(1.0f / newZoom);
    }
}
