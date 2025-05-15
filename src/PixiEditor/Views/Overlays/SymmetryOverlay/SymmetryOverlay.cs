using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.UI.Overlays;
using Drawie.Numerics;
using PixiEditor.Views.Overlays.Handles;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Overlays.SymmetryOverlay;
#nullable enable
internal class SymmetryOverlay : Overlay
{
    public static readonly StyledProperty<double> HorizontalAxisYProperty =
        AvaloniaProperty.Register<SymmetryOverlay, double>(nameof(HorizontalAxisY), defaultValue: 0.0);

    public double HorizontalAxisY
    {
        get => GetValue(HorizontalAxisYProperty);
        set => SetValue(HorizontalAxisYProperty, value);
    }

    public static readonly StyledProperty<double> VerticalAxisXProperty =
        AvaloniaProperty.Register<SymmetryOverlay, double>(nameof(VerticalAxisX), defaultValue: 0.0);

    public double VerticalAxisX
    {
        get => GetValue(VerticalAxisXProperty);
        set => SetValue(VerticalAxisXProperty, value);
    }

    public static readonly StyledProperty<bool> HorizontalAxisVisibleProperty =
        AvaloniaProperty.Register<SymmetryOverlay, bool>(nameof(HorizontalAxisVisible), defaultValue: false);

    public bool HorizontalAxisVisible
    {
        get => GetValue(HorizontalAxisVisibleProperty);
        set => SetValue(HorizontalAxisVisibleProperty, value);
    }

    public static readonly StyledProperty<bool> VerticalAxisVisibleProperty =
        AvaloniaProperty.Register<SymmetryOverlay, bool>(nameof(VerticalAxisVisible), defaultValue: false);

    public bool VerticalAxisVisible
    {
        get => GetValue(VerticalAxisVisibleProperty);
        set => SetValue(VerticalAxisVisibleProperty, value);
    }

    public static readonly StyledProperty<ICommand?> DragCommandProperty =
        AvaloniaProperty.Register<SymmetryOverlay, ICommand?>(nameof(DragCommand));

    public ICommand? DragCommand
    {
        get => GetValue(DragCommandProperty);
        set => SetValue(DragCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> DragEndCommandProperty =
        AvaloniaProperty.Register<SymmetryOverlay, ICommand?>(nameof(DragEndCommand));

    public ICommand? DragEndCommand
    {
        get => GetValue(DragEndCommandProperty);
        set => SetValue(DragEndCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand?> DragStartCommandProperty =
        AvaloniaProperty.Register<SymmetryOverlay, ICommand?>(nameof(DragStartCommand));

    public ICommand? DragStartCommand
    {
        get => GetValue(DragStartCommandProperty);
        set => SetValue(DragStartCommandProperty, value);
    }

    private SymmetryAxisDirection? capturedDirection;
    private SymmetryAxisDirection? hoveredDirection;

    private const double HandleSize = 12;
    private VectorPath handleGeometry = Handle.GetHandleGeometry("MarkerHandle").Path;

    private const float DashWidth = 10.0f;
    const int RulerOffset = -35;
    const int RulerWidth = 4;

    private Paint rulerPen = new Paint() {Color = Colors.White, StrokeWidth = 1, IsAntiAliased = true, Style = PaintStyle.Stroke}; 
    private Paint borderPen = new Paint { Color = Drawie.Backend.Core.ColorsImpl.Color.FromRgb(200, 200, 200), StrokeWidth = 0.25f, IsAntiAliased = true, Style = PaintStyle.Stroke };
    private Paint checkerBlack = new Paint() { Color = Colors.Black, StrokeWidth = 1, IsAntiAliased = true, Style = PaintStyle.Stroke, PathEffect = PathEffect.CreateDash(new float[] { DashWidth, DashWidth }, 0) };
    private Paint checkerWhite = new Paint() { Color = Colors.White, StrokeWidth = 1, IsAntiAliased = true, Style = PaintStyle.Stroke, PathEffect = PathEffect.CreateDash(new float[] { DashWidth, DashWidth }, DashWidth) };
    private Paint textPaint = new Paint() { Color = Colors.White, IsAntiAliased = true, Style = PaintStyle.Fill };
    
    private float PenThickness => 1.0f / (float)ZoomScale;

    private double horizontalAxisY;
    private double verticalAxisX;
    private VecD pointerPosition;

    private VecF lastSize;

    static SymmetryOverlay()
    {
        AffectsRender<SymmetryOverlay>(HorizontalAxisVisibleProperty);
        AffectsRender<SymmetryOverlay>(VerticalAxisVisibleProperty);
        AffectsRender<SymmetryOverlay>(ZoomScaleProperty);

        HorizontalAxisYProperty.Changed.Subscribe(OnPositionUpdate);
        VerticalAxisXProperty.Changed.Subscribe(OnPositionUpdate);
    }

    public override void RenderOverlay(Canvas drawingContext, RectD canvasBounds)
    {
        if (!HorizontalAxisVisible && !VerticalAxisVisible)
            return;

        VecF size = (VecF)canvasBounds.Size;
        lastSize = size;
        checkerBlack.StrokeWidth = PenThickness;
        float dashWidth = DashWidth / (float)ZoomScale;
        checkerWhite.PathEffect?.Dispose();
        checkerWhite.PathEffect = PathEffect.CreateDash(new float[] { dashWidth, dashWidth }, dashWidth);
        
        checkerBlack.PathEffect?.Dispose();
        checkerBlack.PathEffect = PathEffect.CreateDash(new float[] { dashWidth, dashWidth }, 0);
        checkerWhite.StrokeWidth = PenThickness;
        rulerPen.StrokeWidth = PenThickness;

        if (HorizontalAxisVisible)
        {
            if (capturedDirection == SymmetryAxisDirection.Horizontal || hoveredDirection == SymmetryAxisDirection.Horizontal)
            {
                if (horizontalAxisY != 0)
                {
                    DrawHorizontalRuler(drawingContext, false, size);
                }

                if (horizontalAxisY != (int)size.Y)
                {
                    DrawHorizontalRuler(drawingContext, true, size);
                }
            }

            int save = drawingContext.Save();
            drawingContext.Translate(0, (float)horizontalAxisY);
            drawingContext.Scale((float)HandleSize / (float)ZoomScale, (float)HandleSize / (float)ZoomScale);
            
            drawingContext.DrawPath(handleGeometry, borderPen);
            
            drawingContext.RestoreToCount(save);
            
            save = drawingContext.Save();
            drawingContext.Translate(0, (float)horizontalAxisY);
            drawingContext.RotateDegrees(180, size.X / 2, 0);
            drawingContext.Scale((float)HandleSize / (float)ZoomScale, (float)HandleSize / (float)ZoomScale);
            drawingContext.DrawPath(handleGeometry, borderPen);

            drawingContext.RestoreToCount(save);

            drawingContext.DrawLine(new(0, horizontalAxisY), new(size.X, horizontalAxisY), checkerBlack);
            drawingContext.DrawLine(new(0, horizontalAxisY), new(size.X, horizontalAxisY), checkerWhite);
        }
        if (VerticalAxisVisible)
        {
            if (capturedDirection == SymmetryAxisDirection.Vertical || hoveredDirection == SymmetryAxisDirection.Vertical)
            {
                if (verticalAxisX != 0)
                {
                    DrawVerticalRuler(drawingContext, false, size);
                }

                if (verticalAxisX != (int)size.X)
                {
                    DrawVerticalRuler(drawingContext, true, size);
                }
            }

            int saved = drawingContext.Save();
            
            drawingContext.RotateDegrees(90);
            drawingContext.Translate(0, (float)-verticalAxisX);
            drawingContext.Scale((float)HandleSize / (float)ZoomScale, (float)HandleSize / (float)ZoomScale);
            
            drawingContext.DrawPath(handleGeometry, borderPen);
            
            drawingContext.RestoreToCount(saved);
            
            saved = drawingContext.Save();
            drawingContext.RotateDegrees(90);
            drawingContext.Translate(0, (float)-verticalAxisX);
            drawingContext.RotateDegrees(180, size.Y / 2, 0);
            drawingContext.Scale((float)HandleSize / (float)ZoomScale, (float)HandleSize / (float)ZoomScale);
            drawingContext.DrawPath(handleGeometry, borderPen);

            drawingContext.RestoreToCount(saved);

            drawingContext.DrawLine(new(verticalAxisX, 0), new(verticalAxisX, size.Y), checkerBlack);
            drawingContext.DrawLine(new(verticalAxisX, 0), new(verticalAxisX, size.Y), checkerWhite);
        }
    }

    private void DrawHorizontalRuler(Canvas drawingContext, bool upper, VecF size)
    {
        double start = upper ? size.Y : 0;
        bool drawRight = pointerPosition.X > size.X / 2;
        double xOffset = drawRight ? size.X - RulerOffset * PenThickness * 2 : 0;

        drawingContext.DrawLine(new VecD(RulerOffset * PenThickness + xOffset, start), new VecD(RulerOffset * PenThickness + xOffset, horizontalAxisY), rulerPen);
        drawingContext.DrawLine(new VecD((RulerOffset - RulerWidth) * PenThickness + xOffset, start), new VecD((RulerOffset + RulerWidth) * PenThickness + xOffset, start), rulerPen);
        drawingContext.DrawLine(new VecD((RulerOffset - RulerWidth) * PenThickness + xOffset, horizontalAxisY), new VecD((RulerOffset + RulerWidth) * PenThickness + xOffset, horizontalAxisY), rulerPen);

        string text = upper ? $"{start - horizontalAxisY}{new LocalizedString("PIXEL_UNIT")} ({(start - horizontalAxisY) / size.Y * 100:F1}%)" : $"{horizontalAxisY}{new LocalizedString("PIXEL_UNIT")} ({horizontalAxisY / size.Y * 100:F1}%)";

        using Font font = Font.CreateDefault(14f / (float)ZoomScale);
        
        if (size.Y < font.Size * 2.5 || horizontalAxisY == (int)size.Y && upper || horizontalAxisY == 0 && !upper)
        {
            return;
        }

        double textY = horizontalAxisY / 2.0 - font.Size / 2;

        if (upper)
        {
            textY += size.Y / 2f;
        }

        drawingContext.DrawText(text, new VecD(RulerOffset * PenThickness - (drawRight ? -1 : 1) + xOffset, textY), drawRight ? TextAlign.Left : TextAlign.Right, font, textPaint);
    }

    private void DrawVerticalRuler(Canvas drawingContext, bool right, VecF size)
    {
        double start = right ? size.X : 0;
        bool drawBottom = pointerPosition.Y > size.Y / 2;
        double yOffset = drawBottom ? size.Y - RulerOffset * PenThickness * 2 : 0;

        drawingContext.DrawLine(new VecD(start, RulerOffset * PenThickness + yOffset), new VecD(verticalAxisX, RulerOffset * PenThickness + yOffset), rulerPen);
        drawingContext.DrawLine(new VecD(start, (RulerOffset - RulerWidth) * PenThickness + yOffset), new VecD(start, (RulerOffset + RulerWidth) * PenThickness + yOffset), rulerPen);
        drawingContext.DrawLine(new VecD(verticalAxisX, (RulerOffset - RulerWidth) * PenThickness + yOffset), new VecD(verticalAxisX, (RulerOffset + RulerWidth) * PenThickness + yOffset), rulerPen);

        string text = right ? $"{start - verticalAxisX}{new LocalizedString("PIXEL_UNIT")} ({(start - verticalAxisX) / size.X * 100:F1}%)" : $"{verticalAxisX}{new LocalizedString("PIXEL_UNIT")} ({verticalAxisX / size.X * 100:F1}%)";

        using Font font = Font.CreateDefault(14f / (float)ZoomScale);
        
        if (size.X < font.MeasureText(text) * 2.5 || verticalAxisX == (int)size.X && right || verticalAxisX == 0 && !right)
        {
            return;
        }

        double textX = verticalAxisX / 2;

        if (right)
        {
            textX += size.X / 2;
        }

        double textY = RulerOffset * PenThickness - ((drawBottom ? 5 : 2 + font.Size) * PenThickness) + yOffset;
        drawingContext.DrawText(text, new VecD(textX, textY), TextAlign.Center, font, textPaint);
    }

    public override bool TestHit(VecD point)
    {
        return IsTouchingHandle(point) is not null;
    }

    private SymmetryAxisDirection? IsTouchingHandle(VecD position)
    {
        double radius = HandleSize * 4 / ZoomScale / 2;
        VecD left = new(-radius, horizontalAxisY);
        VecD right = new(lastSize.X + radius, horizontalAxisY);
        VecD up = new(verticalAxisX, -radius);
        VecD down = new(verticalAxisX, lastSize.Y + radius);

        if (HorizontalAxisVisible && (Math.Abs((left - position).LongestAxis) < radius || Math.Abs((right - position).LongestAxis) < radius))
            return SymmetryAxisDirection.Horizontal;
        if (VerticalAxisVisible && (Math.Abs((up - position).LongestAxis) < radius || Math.Abs((down - position).LongestAxis) < radius))
            return SymmetryAxisDirection.Vertical;
        return null;
    }

    private VecD ToVecD(Point pos) => new VecD(pos.X, pos.Y);

    private void UpdateHovered(SymmetryAxisDirection? direction)
    {
        Cursor = (hoveredDirection ?? capturedDirection) switch
        {
            SymmetryAxisDirection.Horizontal => new Cursor(StandardCursorType.SizeNorthSouth),
            SymmetryAxisDirection.Vertical => new Cursor(StandardCursorType.SizeWestEast),
            _ => null 
        };

        if (hoveredDirection == direction)
            return;

        hoveredDirection = direction;
        Refresh();
    }

    protected override void OnOverlayPointerPressed(OverlayPointerArgs args)
    {
        if (args.PointerButton != MouseButton.Left)
            return;

        var dir = IsTouchingHandle(args.Point);
        if (dir is null)
            return;
        
        capturedDirection = dir.Value;
        args.Pointer.Capture(this);
        CallSymmetryDragStartCommand(dir.Value);

        args.Handled = TestHit(args.Point);
    }

    protected override void OnOverlayPointerEntered(OverlayPointerArgs args)
    {
        pointerPosition = args.Point;
        var dir = IsTouchingHandle(pointerPosition);
        UpdateHovered(dir);
    }

    protected override void OnOverlayPointerMoved(OverlayPointerArgs args)
    {
        UpdateHovered(IsTouchingHandle(args.Point));

        if (capturedDirection is null)
            return;
        if (capturedDirection == SymmetryAxisDirection.Horizontal)
        {
            horizontalAxisY = Math.Round(Math.Clamp(args.Point.Y, 0, lastSize.Y) * 2) / 2;

            if (args.Modifiers.HasFlag(KeyModifiers.Shift))
            {
                double temp = Math.Round(horizontalAxisY / lastSize.Y * 8) / 8 * lastSize.Y;
                horizontalAxisY = Math.Round(temp * 2) / 2;
            }

            CallSymmetryDragCommand((SymmetryAxisDirection)capturedDirection, horizontalAxisY);
        }
        else if (capturedDirection == SymmetryAxisDirection.Vertical)
        {
            verticalAxisX = Math.Round(Math.Clamp(args.Point.X, 0, lastSize.X) * 2) / 2;

            if (args.Modifiers.HasFlag(KeyModifiers.Control))
            {

                double temp = Math.Round(verticalAxisX / lastSize.X * 8) / 8 * lastSize.X;
                verticalAxisX = Math.Round(temp * 2) / 2;
            }

            CallSymmetryDragCommand((SymmetryAxisDirection)capturedDirection, verticalAxisX);
        }
        
        Refresh();
    }

    protected override void OnOverlayPointerExited(OverlayPointerArgs args)
    {
        UpdateHovered(null);
    }

    protected override void OnOverlayPointerReleased(OverlayPointerArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        if (capturedDirection is null)
            return;

        e.Pointer.Capture(null);

        CallSymmetryDragEndCommand((SymmetryAxisDirection)capturedDirection);

        capturedDirection = null;
        UpdateHovered(IsTouchingHandle(e.Point));
        // Not calling invalidate visual might result in ruler not disappearing when releasing the mouse over the canvas
        Refresh();
    }

    private void CallSymmetryDragCommand(SymmetryAxisDirection direction, double position)
    {
        SymmetryAxisDragInfo dragInfo = new(direction, position);
        if (DragCommand is not null && DragCommand.CanExecute(dragInfo))
            DragCommand.Execute(dragInfo);
    }
    private void CallSymmetryDragEndCommand(SymmetryAxisDirection direction)
    {
        if (DragEndCommand is not null && DragEndCommand.CanExecute(direction))
            DragEndCommand.Execute(direction);
    }
    private void CallSymmetryDragStartCommand(SymmetryAxisDirection direction)
    {
        if (DragStartCommand is not null && DragStartCommand.CanExecute(direction))
            DragStartCommand.Execute(direction);
    }

    private static void OnPositionUpdate(AvaloniaPropertyChangedEventArgs<double> e)
    {
        var self = (SymmetryOverlay)e.Sender;
        self.horizontalAxisY = self.HorizontalAxisY;
        self.verticalAxisX = self.VerticalAxisX;
        self.Refresh();
    }
}
