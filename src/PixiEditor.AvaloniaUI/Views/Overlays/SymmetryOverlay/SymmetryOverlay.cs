using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Hardware.Info;
using PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Views.Overlays.SymmetryOverlay;
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

    public static readonly StyledProperty<double> ZoomboxScaleProperty =
        AvaloniaProperty.Register<SymmetryOverlay, double>(nameof(ZoomboxScale), defaultValue: 1.0);

    public double ZoomboxScale
    {
        get => GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
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

    static SymmetryOverlay()
    {
        AffectsRender<SymmetryOverlay>(HorizontalAxisVisibleProperty);
        AffectsRender<SymmetryOverlay>(VerticalAxisVisibleProperty);
        AffectsRender<SymmetryOverlay>(ZoomboxScaleProperty);

        HorizontalAxisYProperty.Changed.Subscribe(OnPositionUpdate);
        VerticalAxisXProperty.Changed.Subscribe(OnPositionUpdate);
    }

    private const double HandleSize = 12;
    private Geometry handleGeometry /*= GetHandleGeometry("MarkerHandle")*/;

    private const double DashWidth = 10.0;
    const int RulerOffset = -35;
    const int RulerWidth = 4;

    private Brush handleFill = new SolidColorBrush(Brushes.Transparent.Color, 0);
    private Pen rulerPen = new(Brushes.White, 1.0);
    private Pen borderPen = new(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 1.0);
    private Pen checkerBlack = new(new SolidColorBrush(Color.FromRgb(170, 170, 170)), 1.0) { DashStyle = new DashStyle(new[] { DashWidth, DashWidth }, 0) };
    private Pen checkerWhite = new(new SolidColorBrush(Color.FromRgb(100, 100, 100)), 1.0) { DashStyle = new DashStyle(new[] { DashWidth, DashWidth }, DashWidth) };

    private double PenThickness => 1.0 / ZoomboxScale;

    private double horizontalAxisY;
    private double verticalAxisX;
    private Point pointerPosition;

    private MouseUpdateController mouseUpdateController;

    public SymmetryOverlay()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController = new MouseUpdateController(this, MouseMoved);
        PointerEntered += OnPointerEntered;
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        pointerPosition = e.GetPosition(this);
    }

    public override void Render(DrawingContext drawingContext)
    {
        base.Render(drawingContext);
        if (!HorizontalAxisVisible && !VerticalAxisVisible)
            return;

        borderPen.Thickness = 3 * PenThickness;
        checkerBlack.Thickness = PenThickness;
        checkerWhite.Thickness = PenThickness;
        rulerPen.Thickness = PenThickness;

        handleGeometry.Transform = new ScaleTransform(HandleSize / ZoomboxScale, HandleSize / ZoomboxScale);

        if (HorizontalAxisVisible)
        {
            if (capturedDirection == SymmetryAxisDirection.Horizontal || hoveredDirection == SymmetryAxisDirection.Horizontal)
            {
                if (horizontalAxisY != 0)
                {
                    DrawHorizontalRuler(drawingContext, false);
                }

                if (horizontalAxisY != (int)Bounds.Height)
                {
                    DrawHorizontalRuler(drawingContext, true);
                }
            }

            var transformState = drawingContext.PushTransform(new TranslateTransform(0, horizontalAxisY).Value);
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);
            var rotateState = drawingContext.PushTransform(new RotateTransform(180, Bounds.Width / 2, 0).Value);
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);

            rotateState.Dispose();
            transformState.Dispose();

            drawingContext.DrawLine(checkerBlack, new(0, horizontalAxisY), new(Bounds.Width, horizontalAxisY));
            drawingContext.DrawLine(checkerWhite, new(0, horizontalAxisY), new(Bounds.Width, horizontalAxisY));
        }
        if (VerticalAxisVisible)
        {
            if (capturedDirection == SymmetryAxisDirection.Vertical || hoveredDirection == SymmetryAxisDirection.Vertical)
            {
                if (verticalAxisX != 0)
                {
                    DrawVerticalRuler(drawingContext, false);
                }

                if (verticalAxisX != (int)Bounds.Width)
                {
                    DrawVerticalRuler(drawingContext, true);
                }
            }



            var rotation = drawingContext.PushTransform(new RotateTransform(90).Value);
            var translation = drawingContext.PushTransform(new TranslateTransform(0, -verticalAxisX).Value);
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);
            var rotation1 = drawingContext.PushTransform(new RotateTransform(180, Bounds.Height / 2, 0).Value);
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);

            rotation1.Dispose();
            translation.Dispose();
            rotation.Dispose();

            drawingContext.DrawLine(checkerBlack, new(verticalAxisX, 0), new(verticalAxisX, Bounds.Height));
            drawingContext.DrawLine(checkerWhite, new(verticalAxisX, 0), new(verticalAxisX, Bounds.Height));
        }
    }

    private void DrawHorizontalRuler(DrawingContext drawingContext, bool upper)
    {
        double start = upper ? Bounds.Height : 0;
        bool drawRight = pointerPosition.X > Bounds.Width / 2;
        double xOffset = drawRight ? Bounds.Width - RulerOffset * PenThickness * 2 : 0;

        drawingContext.DrawLine(rulerPen, new Point(RulerOffset * PenThickness + xOffset, start), new Point(RulerOffset * PenThickness + xOffset, horizontalAxisY));
        drawingContext.DrawLine(rulerPen, new Point((RulerOffset - RulerWidth) * PenThickness + xOffset, start), new Point((RulerOffset + RulerWidth) * PenThickness + xOffset, start));
        drawingContext.DrawLine(rulerPen, new Point((RulerOffset - RulerWidth) * PenThickness + xOffset, horizontalAxisY), new Point((RulerOffset + RulerWidth) * PenThickness + xOffset, horizontalAxisY));

        string text = upper ? $"{start - horizontalAxisY}{new LocalizedString("PIXEL_UNIT")} ({(start - horizontalAxisY) / Bounds.Height * 100:F1}%)‎" : $"{horizontalAxisY}{new LocalizedString("PIXEL_UNIT")} ({horizontalAxisY / Bounds.Height * 100:F1}%)‎";

        var formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
            ILocalizationProvider.Current.CurrentLanguage.FlowDirection, new Typeface("Segeo UI"), 14.0 / ZoomboxScale, Brushes.White);

        if (Bounds.Height < formattedText.Height * 2.5 || horizontalAxisY == (int)Bounds.Height && upper || horizontalAxisY == 0 && !upper)
        {
            return;
        }

        formattedText.TextAlignment = drawRight ? TextAlignment.Left : TextAlignment.Right;

        double textY = horizontalAxisY / 2.0 - formattedText.Height / 2;

        if (upper)
        {
            textY += Bounds.Height / 2;
        }

        drawingContext.DrawText(formattedText, new Point(RulerOffset * PenThickness - (drawRight ? -1 : 1) + xOffset, textY));
    }

    private void DrawVerticalRuler(DrawingContext drawingContext, bool right)
    {
        double start = right ? Bounds.Width : 0;
        bool drawBottom = pointerPosition.Y > Bounds.Height / 2;
        double yOffset = drawBottom ? Bounds.Height - RulerOffset * PenThickness * 2 : 0;

        drawingContext.DrawLine(rulerPen, new Point(start, RulerOffset * PenThickness + yOffset), new Point(verticalAxisX, RulerOffset * PenThickness + yOffset));
        drawingContext.DrawLine(rulerPen, new Point(start, (RulerOffset - RulerWidth) * PenThickness + yOffset), new Point(start, (RulerOffset + RulerWidth) * PenThickness + yOffset));
        drawingContext.DrawLine(rulerPen, new Point(verticalAxisX, (RulerOffset - RulerWidth) * PenThickness + yOffset), new Point(verticalAxisX, (RulerOffset + RulerWidth) * PenThickness + yOffset));

        string text = right ? $"{start - verticalAxisX}{new LocalizedString("PIXEL_UNIT")} ({(start - verticalAxisX) / Bounds.Width * 100:F1}%)‎" : $"{verticalAxisX}{new LocalizedString("PIXEL_UNIT")} ({verticalAxisX / Bounds.Width * 100:F1}%)‎";

        var formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
            ILocalizationProvider.Current.CurrentLanguage.FlowDirection, new Typeface("Segeo UI"), 14.0 / ZoomboxScale, Brushes.White);

        if (Bounds.Width < formattedText.Width * 2.5 || verticalAxisX == (int)Bounds.Width && right || verticalAxisX == 0 && !right)
        {
            return;
        }

        formattedText.TextAlignment = TextAlignment.Center;

        double textX = verticalAxisX / 2.0;

        if (right)
        {
            textX += Bounds.Width / 2;
        }

        drawingContext.DrawText(formattedText, new Point(textX, RulerOffset * PenThickness - (drawBottom ? -0.7 : 0.3 + formattedText.Height) + yOffset));
    }

    //TODO: I didn't find HitTestCore in Avalonia
    /*protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
    {
        // prevent the line from blocking mouse input
        var point = hitTestParameters.HitPoint;
        if (point.X > 0 && point.Y > 0 && point.X < Bounds.Width && point.Y < Bounds.Height)
            return null;

        return new PointHitTestResult(this, hitTestParameters.HitPoint);
    }*/

    private SymmetryAxisDirection? IsTouchingHandle(VecD position)
    {
        double radius = HandleSize * 4 / ZoomboxScale / 2;
        VecD left = new(-radius, horizontalAxisY);
        VecD right = new(Bounds.Width + radius, horizontalAxisY);
        VecD up = new(verticalAxisX, -radius);
        VecD down = new(verticalAxisX, Bounds.Height + radius);

        if (HorizontalAxisVisible && (Math.Abs((left - position).LongestAxis) < radius || Math.Abs((right - position).LongestAxis) < radius))
            return SymmetryAxisDirection.Horizontal;
        if (VerticalAxisVisible && (Math.Abs((up - position).LongestAxis) < radius || Math.Abs((down - position).LongestAxis) < radius))
            return SymmetryAxisDirection.Vertical;
        return null;
    }

    private VecD ToVecD(Point pos) => new VecD(pos.X, pos.Y);

    private SymmetryAxisDirection? capturedDirection;
    private SymmetryAxisDirection? hoveredDirection;

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
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        MouseButton button = e.GetCurrentPoint(this).Properties.PointerUpdateKind switch
        {
            PointerUpdateKind.LeftButtonPressed => MouseButton.Left,
            PointerUpdateKind.RightButtonPressed => MouseButton.Right,
            PointerUpdateKind.MiddleButtonPressed => MouseButton.Middle,
            _ => MouseButton.None
        };

        if (button != MouseButton.Left)
            return;

        var rawPoint = e.GetPosition(this);
        var pos = ToVecD(rawPoint);
        var dir = IsTouchingHandle(pos);
        if (dir is null)
            return;
        capturedDirection = dir.Value;
        e.Pointer.Capture(this);
        e.Handled = true;
        CallSymmetryDragStartCommand(dir.Value);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        var pos = ToVecD(e.GetPosition(this));
        var dir = IsTouchingHandle(pos);
        UpdateHovered(dir);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        UpdateHovered(null);
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

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton != MouseButton.Left)
            return;

        if (capturedDirection is null)
            return;

        e.Pointer.Capture(null);

        CallSymmetryDragEndCommand((SymmetryAxisDirection)capturedDirection);

        capturedDirection = null;
        UpdateHovered(IsTouchingHandle(ToVecD(e.GetPosition(this))));
        // Not calling invalidate visual might result in ruler not disappearing when releasing the mouse over the canvas 
        InvalidateVisual();
        e.Handled = true;
    }

    protected void MouseMoved(PointerEventArgs e)
    {
        var rawPoint = e.GetPosition(this);
        var pos = ToVecD(rawPoint);
        UpdateHovered(IsTouchingHandle(pos));

        if (capturedDirection is null)
            return;
        if (capturedDirection == SymmetryAxisDirection.Horizontal)
        {
            horizontalAxisY = Math.Round(Math.Clamp(pos.Y, 0, Bounds.Height) * 2) / 2;

            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                double temp = Math.Round(horizontalAxisY / Bounds.Height * 8) / 8 * Bounds.Height;
                horizontalAxisY = Math.Round(temp * 2) / 2;
            }

            CallSymmetryDragCommand((SymmetryAxisDirection)capturedDirection, horizontalAxisY);
        }
        else if (capturedDirection == SymmetryAxisDirection.Vertical)
        {
            verticalAxisX = Math.Round(Math.Clamp(pos.X, 0, Bounds.Width) * 2) / 2;

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {

                double temp = Math.Round(verticalAxisX / Bounds.Width * 8) / 8 * Bounds.Width;
                verticalAxisX = Math.Round(temp * 2) / 2;
            }

            CallSymmetryDragCommand((SymmetryAxisDirection)capturedDirection, verticalAxisX);
        }

        e.Handled = true;
    }

    private static void OnPositionUpdate(AvaloniaPropertyChangedEventArgs<double> e)
    {
        var self = (SymmetryOverlay)e.Sender;
        self.horizontalAxisY = self.HorizontalAxisY;
        self.verticalAxisX = self.VerticalAxisX;
        self.InvalidateVisual();
    }
}
