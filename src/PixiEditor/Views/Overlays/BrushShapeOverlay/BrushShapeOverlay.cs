using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.ViewModels.BrushSystem;
using PixiEditor.Views.Rendering;
using Brush = PixiEditor.Models.BrushEngine.Brush;
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

    public static readonly StyledProperty<StabilizationMode> StabilizationModeProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, StabilizationMode>(
            nameof(StabilizationMode));

    public static readonly StyledProperty<double> StabilizationProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, double>(
            nameof(Stabilization));

    public static readonly StyledProperty<VecD> LastAppliedPointProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, VecD>(
            nameof(LastAppliedPoint));

    public VecD LastAppliedPoint
    {
        get => GetValue(LastAppliedPointProperty);
        set => SetValue(LastAppliedPointProperty, value);
    }

    public double Stabilization
    {
        get => GetValue(StabilizationProperty);
        set => SetValue(StabilizationProperty, value);
    }

    public StabilizationMode StabilizationMode
    {
        get => GetValue(StabilizationModeProperty);
        set => SetValue(StabilizationModeProperty, value);
    }

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

    public static readonly StyledProperty<ExecutionTrigger<string>> BrushSettingChangedTriggerProperty =
        AvaloniaProperty.Register<BrushShapeOverlay, ExecutionTrigger<string>>(
            nameof(BrushSettingChangedTrigger));

    public ExecutionTrigger<string> BrushSettingChangedTrigger
    {
        get => GetValue(BrushSettingChangedTriggerProperty);
        set => SetValue(BrushSettingChangedTriggerProperty, value);
    }

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

    private VecD lastPoint;
    private VecD lastDirCalculationPoint;
    private float lastSize;
    private bool isMouseDown;
    private PointerInfo lastPointerInfo;

    private ChangeableDocument.Changeables.Brushes.BrushEngine engine = new();

    private Drawie.Backend.Core.ColorsImpl.Color ropeColor;
    private Drawie.Backend.Core.ColorsImpl.Color pointColor;

    static BrushShapeOverlay()
    {
        AffectsOverlayRender(BrushShapeProperty, BrushDataProperty, ActiveFrameTimeProperty, EditorDataProperty);
        BrushDataProperty.Changed.AddClassHandler<BrushShapeOverlay>((overlay, args) =>
        {
            UpdateBrush(args);
        });
        BrushSettingChangedTriggerProperty.Changed.AddClassHandler<BrushShapeOverlay>((overlay, args) =>
        {
            if (args.OldValue is ExecutionTrigger<string> oldTrigger)
            {
                oldTrigger.Triggered -= overlay.Triggered;
            }

            if (args.NewValue is ExecutionTrigger<string> trigger)
            {
                trigger.Triggered += overlay.Triggered;
            }
        });
        ActiveFrameTimeProperty.Changed.AddClassHandler<BrushShapeOverlay>((overlay, args) => UpdateBrush(args));
        EditorDataProperty.Changed.AddClassHandler<BrushShapeOverlay>((overlay, args) => UpdateBrush(args));
    }

    public BrushShapeOverlay()
    {
        IsHitTestVisible = false;
        AlwaysPassPointerEvents = true;
        ropeColor = ResourceLoader.GetResource<Color>("ErrorOnDarkColor").ToColor();
        pointColor = ResourceLoader.GetResource<Color>("ThemeAccent3Color").ToColor();
    }

    private void Triggered(object? sender, string s)
    {
        ExecuteBrush(lastDirCalculationPoint);
        UpdateBrushShape(lastDirCalculationPoint);
        Refresh();
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

        PointerInfo pointer = new PointerInfo(pos, 1, 0, VecD.Zero, dirNormalized, true, false);

        engine.ExecuteBrush(null, BrushData, pos, ActiveFrameTime,
            ColorSpace.CreateSrgb(), SamplingOptions.Default, pointer, new KeyboardInfo(),
            EditorData?.Invoke() ?? new EditorData(Colors.White, Colors.Black));
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        isMouseDown = args.Properties.IsLeftButtonPressed;
        if (!args.Properties.IsLeftButtonPressed && BrushData.BrushGraph != null)
        {
            ExecuteBrush(args.Point);
        }

        UpdateBrushShape(args.Point);
        lastPoint = args.Point;

        Refresh();
    }

    protected override void OnOverlayPointerPressed(OverlayPointerArgs args)
    {
        isMouseDown = true;
    }

    protected override void OnOverlayPointerReleased(OverlayPointerArgs args)
    {
        isMouseDown = false;
    }

    protected override void OnOverlayPointerExited(OverlayPointerArgs args)
    {
        isMouseDown = false;
    }

    protected override void OnOverlayPointerEntered(OverlayPointerArgs args)
    {
        isMouseDown = args.Properties.IsLeftButtonPressed;
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

            if (isMouseDown)
            {
                if (StabilizationMode == StabilizationMode.CircleRope)
                {
                    float radius = (float)Stabilization / (float)ZoomScale;
                    paint.Style = PaintStyle.Stroke;

                    paint.Color = pointColor;
                    targetCanvas.DrawCircle(LastAppliedPoint, 5f / (float)ZoomScale, paint);

                    paint.Color = ropeColor;

                    DrawConstrainedRope(targetCanvas, lastPoint, LastAppliedPoint, radius, paint);

                    paint.Color = pointColor;
                    targetCanvas.DrawCircle(lastPoint, 5f / (float)ZoomScale, paint);
                }
                else if (StabilizationMode == StabilizationMode.TimeBased)
                {
                    paint.Style = PaintStyle.Stroke;

                    paint.Color = pointColor;
                    targetCanvas.DrawCircle(LastAppliedPoint, 5f / (float)ZoomScale, paint);

                    paint.Color = ropeColor;
                    targetCanvas.DrawLine(LastAppliedPoint, lastPoint, paint);

                    paint.Color = pointColor;
                    targetCanvas.DrawCircle(lastPoint, 5f / (float)ZoomScale, paint);
                }
            }

            if (StabilizationMode == StabilizationMode.None || !isMouseDown)
            {
                paint.Color = Colors.LightGray;
                targetCanvas.DrawPath(BrushShape, paint);
            }

            targetCanvas.Restore();
        }
    }

    protected override void ZoomChanged(double newZoom)
    {
        paint.StrokeWidth = (float)(1.0f / newZoom);
    }

    void DrawConstrainedRope(Canvas targetCanvas, VecD A, VecD B, double radius, Paint paint)
    {
        var AB = B - A;
        double d = AB.Length;

        using var path = new VectorPath();

        if (d >= radius || d <= 1e-9)
        {
            path.MoveTo((VecF)A);
            path.LineTo((VecF)B);
            targetCanvas.DrawPath(path, paint);
            return;
        }

        var dir = AB.Normalize();
        var perp = new VecD(-dir.Y, dir.X);
        var mid = (A + B) * 0.5;

        // compute perpendicular offset so total rope length = radius
        double halfD = d / 2.0;
        double halfR = radius / 2.0;
        double h = Math.Sqrt(Math.Max(0.0, halfR * halfR - halfD * halfD));

        VecD P = mid + perp * h;

        VecD c1 = A + (P - A) * 0.5;
        VecD c2 = B + (P - B) * 0.5;

        path.MoveTo((VecF)A);
        path.CubicTo((VecF)c1, (VecF)c2, (VecF)B);
        targetCanvas.DrawPath(path, paint);
    }
}
