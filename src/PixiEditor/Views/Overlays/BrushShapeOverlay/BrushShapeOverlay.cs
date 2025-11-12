using Avalonia;
using Avalonia.Input;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using PixiEditor.UI.Common.Extensions;
using PixiEditor.Views.Rendering;
using Canvas = Drawie.Backend.Core.Surfaces.Canvas;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

namespace PixiEditor.Views.Overlays.BrushShapeOverlay;
#nullable enable
internal class BrushShapeOverlay : Overlay
{
    public static readonly StyledProperty<Scene> SceneProperty = AvaloniaProperty.Register<BrushShapeOverlay, Scene>(
        nameof(Scene));

    public static readonly StyledProperty<VectorPath?> BrushShapeProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, VectorPath?>(
            nameof(BrushShape));

    public static readonly StyledProperty<KeyFrameTime> ActiveFrameTimeProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, KeyFrameTime>(
            nameof(ActiveFrameTime));

    public static readonly StyledProperty<Func<EditorData>> EditorDataProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, Func<EditorData>>(
            nameof(EditorData));

    public Func<EditorData> EditorData
    {
        get => GetValue(EditorDataProperty);
        set => SetValue(EditorDataProperty, value);
    }

    public KeyFrameTime ActiveFrameTime
    {
        get => GetValue(ActiveFrameTimeProperty);
        set => SetValue(ActiveFrameTimeProperty, value);
    }

    public static readonly StyledProperty<BrushData> BrushDataProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, BrushData>(
            nameof(BrushData));

    public BrushData BrushData
    {
        get => GetValue(BrushDataProperty);
        set => SetValue(BrushDataProperty, value);
    }

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

    private Paint paint = new Paint() { Color = Colors.LightGray, StrokeWidth = 1, Style = PaintStyle.Stroke };

    private VecD lastDirCalculationPoint;
    private float lastSize;
    private PointerInfo lastPointerInfo;

    private ChangeableDocument.Changeables.Brushes.BrushEngine engine = new();

    static BrushShapeOverlay()
    {
        AffectsOverlayRender(BrushShapeProperty, BrushDataProperty, ActiveFrameTimeProperty, EditorDataProperty);
        BrushDataProperty.Changed.AddClassHandler<BrushShapeOverlay>((overlay, args) => UpdateBrush(args));
        ActiveFrameTimeProperty.Changed.AddClassHandler<BrushShapeOverlay>((overlay, args) => UpdateBrush(args));
        EditorDataProperty.Changed.AddClassHandler<BrushShapeOverlay>((overlay, args) => UpdateBrush(args));
    }

    public BrushShapeOverlay()
    {
        IsHitTestVisible = false;
    }

    private static void UpdateBrush(AvaloniaPropertyChangedEventArgs args)
    {
        BrushShapeOverlay overlay = args.Sender as BrushShapeOverlay;
        if (overlay == null) return;
        overlay.UpdateBrushShape(overlay.lastDirCalculationPoint);
        overlay.Refresh();
    }

    private void ExecuteBrush(VecD pos)
    {
        if (VecD.Distance(lastDirCalculationPoint, pos) > 1)
        {
            lastDirCalculationPoint = lastDirCalculationPoint.Lerp(pos, 0.5f);
        }

        VecD dir = lastDirCalculationPoint - pos;
        VecD vecDir = new VecD(dir.X, dir.Y);
        VecD dirNormalized = vecDir.Length > 0 ? vecDir.Normalize() : lastPointerInfo.MovementDirection;

        PointerInfo pointer = new PointerInfo(pos, 1, 0, VecD.Zero, dirNormalized);

        engine.ExecuteBrush(null, BrushData, pos, ActiveFrameTime,
            ColorSpace.CreateSrgb(), SamplingOptions.Default, pointer, new KeyboardInfo(), EditorData?.Invoke() ?? new EditorData(Colors.White, Colors.Black));
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        if (!args.Properties.IsLeftButtonPressed && BrushData.BrushGraph != null)
        {
            ExecuteBrush(args.Point);
        }

        UpdateBrushShape(args.Point);

        Refresh();
    }

    protected override void OnKeyPressed(KeyEventArgs args)
    {
        UpdateBrushShape(lastDirCalculationPoint);
        Refresh();
    }

    private void UpdateBrushShape(VecD pos)
    {
        if (BrushData.BrushGraph == null) return;

        BrushShape = engine.EvaluateShape(pos, BrushData);
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
