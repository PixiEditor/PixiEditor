using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.UI.Overlays;
using PixiEditor.Numerics;
using PixiEditor.Views.Overlays.Handles;
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
    public static readonly StyledProperty<VecI> SizeProperty = AvaloniaProperty.Register<SymmetryOverlay, VecI>(nameof(Size));

    private const double HandleSize = 12;
    private Geometry handleGeometry = Handle.GetHandleGeometry("MarkerHandle");

    private const double DashWidth = 10.0;
    const int RulerOffset = -35;
    const int RulerWidth = 4;

    private Brush handleFill = new SolidColorBrush(Brushes.Transparent.Color, 0);
    private Pen rulerPen = new(Brushes.White, 1.0);
    private Pen borderPen = new(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 1.0);
    private Pen checkerBlack = new(new SolidColorBrush(Color.FromRgb(170, 170, 170)), 1.0) { DashStyle = new DashStyle(new[] { DashWidth, DashWidth }, 0) };
    private Pen checkerWhite = new(new SolidColorBrush(Color.FromRgb(100, 100, 100)), 1.0) { DashStyle = new DashStyle(new[] { DashWidth, DashWidth }, DashWidth) };

    private double PenThickness => 1.0 / ZoomScale;

    public VecI Size    
    {
        get { return (VecI)GetValue(SizeProperty); }
        set { SetValue(SizeProperty, value); }
    }

    private double horizontalAxisY;
    private double verticalAxisX;
    private VecD pointerPosition;

    static SymmetryOverlay()
    {
        AffectsRender<SymmetryOverlay>(HorizontalAxisVisibleProperty);
        AffectsRender<SymmetryOverlay>(VerticalAxisVisibleProperty);
        AffectsRender<SymmetryOverlay>(ZoomScaleProperty);

        HorizontalAxisYProperty.Changed.Subscribe(OnPositionUpdate);
        VerticalAxisXProperty.Changed.Subscribe(OnPositionUpdate);
    }

    public override void RenderOverlay(DrawingContext drawingContext, RectD canvasBounds)
    {
        base.Render(drawingContext);
        if (!HorizontalAxisVisible && !VerticalAxisVisible)
            return;

        borderPen.Thickness = 3 * PenThickness;
        checkerBlack.Thickness = PenThickness;
        checkerWhite.Thickness = PenThickness;
        rulerPen.Thickness = PenThickness;

        handleGeometry.Transform = new ScaleTransform(HandleSize / ZoomScale, HandleSize / ZoomScale);

        if (HorizontalAxisVisible)
        {
            if (capturedDirection == SymmetryAxisDirection.Horizontal || hoveredDirection == SymmetryAxisDirection.Horizontal)
            {
                if (horizontalAxisY != 0)
                {
                    DrawHorizontalRuler(drawingContext, false);
                }

                if (horizontalAxisY != (int)Size.Y)
                {
                    DrawHorizontalRuler(drawingContext, true);
                }
            }

            var transformState = drawingContext.PushTransform(new TranslateTransform(0, horizontalAxisY).Value);
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);
            var rotateState = drawingContext.PushTransform(new RotateTransform(180, Size.X / 2, 0).Value);
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);

            rotateState.Dispose();
            transformState.Dispose();

            drawingContext.DrawLine(checkerBlack, new(0, horizontalAxisY), new(Size.X, horizontalAxisY));
            drawingContext.DrawLine(checkerWhite, new(0, horizontalAxisY), new(Size.X, horizontalAxisY));
        }
        if (VerticalAxisVisible)
        {
            if (capturedDirection == SymmetryAxisDirection.Vertical || hoveredDirection == SymmetryAxisDirection.Vertical)
            {
                if (verticalAxisX != 0)
                {
                    DrawVerticalRuler(drawingContext, false);
                }

                if (verticalAxisX != (int)Size.X)
                {
                    DrawVerticalRuler(drawingContext, true);
                }
            }

            var rotation = drawingContext.PushTransform(new RotateTransform(90).Value);
            var translation = drawingContext.PushTransform(new TranslateTransform(0, -verticalAxisX).Value);
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);
            var rotation1 = drawingContext.PushTransform(new RotateTransform(180, Size.Y / 2, 0).Value);
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);

            rotation1.Dispose();
            translation.Dispose();
            rotation.Dispose();

            drawingContext.DrawLine(checkerBlack, new(verticalAxisX, 0), new(verticalAxisX, Size.Y));
            drawingContext.DrawLine(checkerWhite, new(verticalAxisX, 0), new(verticalAxisX, Size.Y));
        }
    }

    private void DrawHorizontalRuler(DrawingContext drawingContext, bool upper)
    {
        double start = upper ? Size.Y : 0;
        bool drawRight = pointerPosition.X > Size.X / 2;
        double xOffset = drawRight ? Size.X - RulerOffset * PenThickness * 2 : 0;

        drawingContext.DrawLine(rulerPen, new Point(RulerOffset * PenThickness + xOffset, start), new Point(RulerOffset * PenThickness + xOffset, horizontalAxisY));
        drawingContext.DrawLine(rulerPen, new Point((RulerOffset - RulerWidth) * PenThickness + xOffset, start), new Point((RulerOffset + RulerWidth) * PenThickness + xOffset, start));
        drawingContext.DrawLine(rulerPen, new Point((RulerOffset - RulerWidth) * PenThickness + xOffset, horizontalAxisY), new Point((RulerOffset + RulerWidth) * PenThickness + xOffset, horizontalAxisY));

        string text = upper ? $"{start - horizontalAxisY}{new LocalizedString("PIXEL_UNIT")} ({(start - horizontalAxisY) / Size.Y * 100:F1}%)‎" : $"{horizontalAxisY}{new LocalizedString("PIXEL_UNIT")} ({horizontalAxisY / Size.Y * 100:F1}%)‎";

        var formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
            ILocalizationProvider.Current.CurrentLanguage.FlowDirection, new Typeface("Segeo UI"), 14.0 / ZoomScale, Brushes.White);

        if (Size.Y < formattedText.Height * 2.5 || horizontalAxisY == (int)Size.Y && upper || horizontalAxisY == 0 && !upper)
        {
            return;
        }

        formattedText.TextAlignment = drawRight ? TextAlignment.Left : TextAlignment.Right;

        double textY = horizontalAxisY / 2.0 - formattedText.Height / 2;

        if (upper)
        {
            textY += Size.Y / 2;
        }

        drawingContext.DrawText(formattedText, new Point(RulerOffset * PenThickness - (drawRight ? -1 : 1) + xOffset, textY));
    }

    private void DrawVerticalRuler(DrawingContext drawingContext, bool right)
    {
        double start = right ? Size.X : 0;
        bool drawBottom = pointerPosition.Y > Size.Y / 2;
        double yOffset = drawBottom ? Size.Y - RulerOffset * PenThickness * 2 : 0;

        drawingContext.DrawLine(rulerPen, new Point(start, RulerOffset * PenThickness + yOffset), new Point(verticalAxisX, RulerOffset * PenThickness + yOffset));
        drawingContext.DrawLine(rulerPen, new Point(start, (RulerOffset - RulerWidth) * PenThickness + yOffset), new Point(start, (RulerOffset + RulerWidth) * PenThickness + yOffset));
        drawingContext.DrawLine(rulerPen, new Point(verticalAxisX, (RulerOffset - RulerWidth) * PenThickness + yOffset), new Point(verticalAxisX, (RulerOffset + RulerWidth) * PenThickness + yOffset));

        string text = right ? $"{start - verticalAxisX}{new LocalizedString("PIXEL_UNIT")} ({(start - verticalAxisX) / Size.X * 100:F1}%)‎" : $"{verticalAxisX}{new LocalizedString("PIXEL_UNIT")} ({verticalAxisX / Size.X * 100:F1}%)‎";

        var formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
            ILocalizationProvider.Current.CurrentLanguage.FlowDirection, new Typeface("Segeo UI"), 14.0 / ZoomScale, Brushes.White);

        if (Size.X < formattedText.Width * 2.5 || verticalAxisX == (int)Size.X && right || verticalAxisX == 0 && !right)
        {
            return;
        }

        formattedText.TextAlignment = TextAlignment.Center;

        double textX = verticalAxisX / 2.0;

        if (right)
        {
            textX += Size.X / 2;
        }

        drawingContext.DrawText(formattedText, new Point(textX, RulerOffset * PenThickness - (drawBottom ? -0.7 : 0.3 + formattedText.Height) + yOffset));
    }

    public override bool TestHit(VecD point)
    {
        return IsTouchingHandle(point) is not null;
    }

    private SymmetryAxisDirection? IsTouchingHandle(VecD position)
    {
        double radius = HandleSize * 4 / ZoomScale / 2;
        VecD left = new(-radius, horizontalAxisY);
        VecD right = new(Size.X + radius, horizontalAxisY);
        VecD up = new(verticalAxisX, -radius);
        VecD down = new(verticalAxisX, Size.Y + radius);

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
            _ => new Cursor(StandardCursorType.Arrow)
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
            horizontalAxisY = Math.Round(Math.Clamp(args.Point.Y, 0, Size.Y) * 2) / 2;

            if (args.Modifiers.HasFlag(KeyModifiers.Shift))
            {
                double temp = Math.Round(horizontalAxisY / Size.Y * 8) / 8 * Size.Y;
                horizontalAxisY = Math.Round(temp * 2) / 2;
            }

            CallSymmetryDragCommand((SymmetryAxisDirection)capturedDirection, horizontalAxisY);
        }
        else if (capturedDirection == SymmetryAxisDirection.Vertical)
        {
            verticalAxisX = Math.Round(Math.Clamp(args.Point.X, 0, Size.X) * 2) / 2;

            if (args.Modifiers.HasFlag(KeyModifiers.Control))
            {

                double temp = Math.Round(verticalAxisX / Size.X * 8) / 8 * Size.X;
                verticalAxisX = Math.Round(temp * 2) / 2;
            }

            CallSymmetryDragCommand((SymmetryAxisDirection)capturedDirection, verticalAxisX);
        }
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
