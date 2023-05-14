using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ChunkyImageLib.DataHolders;
using PixiEditor;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Localization;
using PixiEditor.Views;
using PixiEditor.Views.UserControls;
using PixiEditor.Views.UserControls.Overlays.SymmetryOverlay;
using PixiEditor.Views.UserControls.SymmetryOverlay;

namespace PixiEditor.Views.UserControls.Overlays.SymmetryOverlay;
#nullable enable
internal class SymmetryOverlay : Control
{
    public static readonly DependencyProperty HorizontalAxisYProperty =
        DependencyProperty.Register(nameof(HorizontalAxisY), typeof(double), typeof(SymmetryOverlay),
            new(0.0, OnPositionUpdate));

    public double HorizontalAxisY
    {
        get => (double)GetValue(HorizontalAxisYProperty);
        set => SetValue(HorizontalAxisYProperty, value);
    }

    public static readonly DependencyProperty VerticalAxisXProperty =
        DependencyProperty.Register(nameof(VerticalAxisX), typeof(double), typeof(SymmetryOverlay),
            new(0.0, OnPositionUpdate));

    public double VerticalAxisX
    {
        get => (double)GetValue(VerticalAxisXProperty);
        set => SetValue(VerticalAxisXProperty, value);
    }

    public static readonly DependencyProperty HorizontalAxisVisibleProperty =
        DependencyProperty.Register(nameof(HorizontalAxisVisible), typeof(bool), typeof(SymmetryOverlay),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool HorizontalAxisVisible
    {
        get => (bool)GetValue(HorizontalAxisVisibleProperty);
        set => SetValue(HorizontalAxisVisibleProperty, value);
    }

    public static readonly DependencyProperty VerticalAxisVisibleProperty =
        DependencyProperty.Register(nameof(VerticalAxisVisible), typeof(bool), typeof(SymmetryOverlay),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool VerticalAxisVisible
    {
        get => (bool)GetValue(VerticalAxisVisibleProperty);
        set => SetValue(VerticalAxisVisibleProperty, value);
    }

    public static readonly DependencyProperty ZoomboxScaleProperty =
        DependencyProperty.Register(nameof(ZoomboxScale), typeof(double), typeof(SymmetryOverlay),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double ZoomboxScale
    {
        get => (double)GetValue(ZoomboxScaleProperty);
        set => SetValue(ZoomboxScaleProperty, value);
    }

    public static readonly DependencyProperty DragCommandProperty =
        DependencyProperty.Register(nameof(DragCommand), typeof(ICommand), typeof(SymmetryOverlay), new(null));

    public ICommand? DragCommand
    {
        get => (ICommand)GetValue(DragCommandProperty);
        set => SetValue(DragCommandProperty, value);
    }

    public static readonly DependencyProperty DragEndCommandProperty =
        DependencyProperty.Register(nameof(DragEndCommand), typeof(ICommand), typeof(SymmetryOverlay), new(null));

    public ICommand? DragEndCommand
    {
        get => (ICommand)GetValue(DragEndCommandProperty);
        set => SetValue(DragEndCommandProperty, value);
    }

    public static readonly DependencyProperty DragStartCommandProperty =
        DependencyProperty.Register(nameof(DragStartCommand), typeof(ICommand), typeof(SymmetryOverlay), new(null));

    public ICommand? DragStartCommand
    {
        get => (ICommand?)GetValue(DragStartCommandProperty);
        set => SetValue(DragStartCommandProperty, value);
    }

    private const double HandleSize = 12;
    private PathGeometry handleGeometry = new()
    {
        FillRule = FillRule.Nonzero,
        Figures = (PathFigureCollection?)new PathFigureCollectionConverter()
            .ConvertFrom($"M -1.1146 -0.6603 c -0.1215 -0.1215 -0.3187 -0.1215 -0.4401 0 l -0.4401 0.4401 c -0.1215 0.1215 -0.1215 0.3187 0 0.4401 l 0.4401 0.4401 c 0.1215 0.1215 0.3187 0.1215 0.4401 0 l 0.4401 -0.4401 c 0.1215 -0.1215 0.1215 -0.3187 0 -0.4401 l -0.4401 -0.4401 Z M -0.5834 0.0012 l 0.5833 -0.0013"),
    };

    private const double DashWidth = 10.0;
    const int RulerOffset = -35;
    const int RulerWidth = 4;

    private Brush handleFill = Brushes.Transparent;
    private Pen rulerPen = new(Brushes.White, 1.0);
    private Pen borderPen = new(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 1.0);
    private Pen checkerBlack = new(new SolidColorBrush(Color.FromRgb(170, 170, 170)), 1.0) { DashStyle = new DashStyle(new[] { DashWidth, DashWidth }, 0) };
    private Pen checkerWhite = new(new SolidColorBrush(Color.FromRgb(100, 100, 100)), 1.0) { DashStyle = new DashStyle(new[] { DashWidth, DashWidth }, DashWidth) };

    private double PenThickness => 1.0 / ZoomboxScale;

    private double horizontalAxisY;
    private double verticalAxisX;

    private MouseUpdateController mouseUpdateController;

    public SymmetryOverlay()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        mouseUpdateController = new MouseUpdateController(this, MouseMoved);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
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

                if (horizontalAxisY != (int)RenderSize.Height)
                {
                    DrawHorizontalRuler(drawingContext, true);
                }
            }

            drawingContext.PushTransform(new TranslateTransform(0, horizontalAxisY));
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);
            drawingContext.PushTransform(new RotateTransform(180, ActualWidth / 2, 0));
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.DrawLine(checkerBlack, new(0, horizontalAxisY), new(ActualWidth, horizontalAxisY));
            drawingContext.DrawLine(checkerWhite, new(0, horizontalAxisY), new(ActualWidth, horizontalAxisY));
        }
        if (VerticalAxisVisible)
        {
            if (capturedDirection == SymmetryAxisDirection.Vertical || hoveredDirection == SymmetryAxisDirection.Vertical)
            {
                if (verticalAxisX != 0)
                {
                    DrawVerticalRuler(drawingContext, false);
                }

                if (verticalAxisX != (int)RenderSize.Width)
                {
                    DrawVerticalRuler(drawingContext, true);
                }
            }

            drawingContext.PushTransform(new RotateTransform(90));
            drawingContext.PushTransform(new TranslateTransform(0, -verticalAxisX));
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);
            drawingContext.PushTransform(new RotateTransform(180, ActualHeight / 2, 0));
            drawingContext.DrawGeometry(handleFill, borderPen, handleGeometry);
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.Pop();
            drawingContext.DrawLine(checkerBlack, new(verticalAxisX, 0), new(verticalAxisX, ActualHeight));
            drawingContext.DrawLine(checkerWhite, new(verticalAxisX, 0), new(verticalAxisX, ActualHeight));
        }
    }

    private void DrawHorizontalRuler(DrawingContext drawingContext, bool upper)
    {
        double start = upper ? RenderSize.Height : 0;
        bool drawRight = Mouse.GetPosition(this).X > RenderSize.Width / 2;
        double xOffset = drawRight ? RenderSize.Width - RulerOffset * PenThickness * 2 : 0;

        drawingContext.DrawLine(rulerPen, new Point(RulerOffset * PenThickness + xOffset, start), new Point(RulerOffset * PenThickness + xOffset, horizontalAxisY));
        drawingContext.DrawLine(rulerPen, new Point((RulerOffset - RulerWidth) * PenThickness + xOffset, start), new Point((RulerOffset + RulerWidth) * PenThickness + xOffset, start));
        drawingContext.DrawLine(rulerPen, new Point((RulerOffset - RulerWidth) * PenThickness + xOffset, horizontalAxisY), new Point((RulerOffset + RulerWidth) * PenThickness + xOffset, horizontalAxisY));

        string text = upper ? $"{start - horizontalAxisY}{new LocalizedString("PIXEL_UNIT")} ({(start - horizontalAxisY) / RenderSize.Height * 100:F1}%)‎" : $"{horizontalAxisY}{new LocalizedString("PIXEL_UNIT")} ({horizontalAxisY / RenderSize.Height * 100:F1}%)‎";

        var formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
            ILocalizationProvider.Current.CurrentLanguage.FlowDirection, new Typeface("Segeo UI"), 14.0 / ZoomboxScale, Brushes.White,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        if (ActualHeight < formattedText.Height * 2.5 || horizontalAxisY == (int)RenderSize.Height && upper || horizontalAxisY == 0 && !upper)
        {
            return;
        }

        formattedText.TextAlignment = drawRight ? TextAlignment.Left : TextAlignment.Right;

        double textY = horizontalAxisY / 2.0 - formattedText.Height / 2;

        if (upper)
        {
            textY += RenderSize.Height / 2;
        }

        drawingContext.DrawText(formattedText, new Point(RulerOffset * PenThickness - (drawRight ? -1 : 1) + xOffset, textY));
    }

    private void DrawVerticalRuler(DrawingContext drawingContext, bool right)
    {
        double start = right ? RenderSize.Width : 0;
        bool drawBottom = Mouse.GetPosition(this).Y > RenderSize.Height / 2;
        double yOffset = drawBottom ? RenderSize.Height - RulerOffset * PenThickness * 2 : 0;

        drawingContext.DrawLine(rulerPen, new Point(start, RulerOffset * PenThickness + yOffset), new Point(verticalAxisX, RulerOffset * PenThickness + yOffset));
        drawingContext.DrawLine(rulerPen, new Point(start, (RulerOffset - RulerWidth) * PenThickness + yOffset), new Point(start, (RulerOffset + RulerWidth) * PenThickness + yOffset));
        drawingContext.DrawLine(rulerPen, new Point(verticalAxisX, (RulerOffset - RulerWidth) * PenThickness + yOffset), new Point(verticalAxisX, (RulerOffset + RulerWidth) * PenThickness + yOffset));

        string text = right ? $"{start - verticalAxisX}{new LocalizedString("PIXEL_UNIT")} ({(start - verticalAxisX) / RenderSize.Width * 100:F1}%)‎" : $"{verticalAxisX}{new LocalizedString("PIXEL_UNIT")} ({verticalAxisX / RenderSize.Width * 100:F1}%)‎";

        var formattedText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"),
            ILocalizationProvider.Current.CurrentLanguage.FlowDirection, new Typeface("Segeo UI"), 14.0 / ZoomboxScale, Brushes.White,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        if (ActualWidth < formattedText.Width * 2.5 || verticalAxisX == (int)RenderSize.Width && right || verticalAxisX == 0 && !right)
        {
            return;
        }

        formattedText.TextAlignment = TextAlignment.Center;

        double textX = verticalAxisX / 2.0;

        if (right)
        {
            textX += RenderSize.Width / 2;
        }

        drawingContext.DrawText(formattedText, new Point(textX, RulerOffset * PenThickness - (drawBottom ? -0.7 : 0.3 + formattedText.Height) + yOffset));
    }

    protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
    {
        // prevent the line from blocking mouse input
        var point = hitTestParameters.HitPoint;
        if (point.X > 0 && point.Y > 0 && point.X < ActualWidth && point.Y < ActualHeight)
            return null;
        return new PointHitTestResult(this, hitTestParameters.HitPoint);
    }

    private SymmetryAxisDirection? IsTouchingHandle(VecD position)
    {
        double radius = HandleSize * 4 / ZoomboxScale / 2;
        VecD left = new(-radius, horizontalAxisY);
        VecD right = new(ActualWidth + radius, horizontalAxisY);
        VecD up = new(verticalAxisX, -radius);
        VecD down = new(verticalAxisX, ActualHeight + radius);

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
            SymmetryAxisDirection.Horizontal => Cursors.SizeNS,
            SymmetryAxisDirection.Vertical => Cursors.SizeWE,
            _ => Cursors.Arrow
        };

        if (hoveredDirection == direction)
            return;

        hoveredDirection = direction;
        InvalidateVisual();
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.ChangedButton != MouseButton.Left)
            return;

        var pos = ToVecD(e.GetPosition(this));
        var dir = IsTouchingHandle(pos);
        if (dir is null)
            return;
        capturedDirection = dir.Value;
        CaptureMouse();
        e.Handled = true;
        CallSymmetryDragStartCommand(dir.Value);
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        var pos = ToVecD(e.GetPosition(this));
        var dir = IsTouchingHandle(pos);
        UpdateHovered(dir);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
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

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.ChangedButton != MouseButton.Left)
            return;

        if (capturedDirection is null)
            return;

        ReleaseMouseCapture();

        CallSymmetryDragEndCommand((SymmetryAxisDirection)capturedDirection);

        capturedDirection = null;
        UpdateHovered(IsTouchingHandle(ToVecD(e.GetPosition(this))));
        // Not calling invalidate visual might result in ruler not disappearing when releasing the mouse over the canvas 
        InvalidateVisual();
        e.Handled = true;
    }

    protected void MouseMoved(object sender, MouseEventArgs e)
    {
        var pos = ToVecD(e.GetPosition(this));
        UpdateHovered(IsTouchingHandle(pos));

        if (capturedDirection is null)
            return;
        if (capturedDirection == SymmetryAxisDirection.Horizontal)
        {
            horizontalAxisY = Math.Round(Math.Clamp(pos.Y, 0, ActualHeight) * 2) / 2;

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                double temp = Math.Round(horizontalAxisY / RenderSize.Height * 8) / 8 * RenderSize.Height;
                horizontalAxisY = Math.Round(temp * 2) / 2;
            }

            CallSymmetryDragCommand((SymmetryAxisDirection)capturedDirection, horizontalAxisY);
        }
        else if (capturedDirection == SymmetryAxisDirection.Vertical)
        {
            verticalAxisX = Math.Round(Math.Clamp(pos.X, 0, ActualWidth) * 2) / 2;

            if (Keyboard.IsKeyDown(Key.LeftShift))
            {

                double temp = Math.Round(verticalAxisX / RenderSize.Width * 8) / 8 * RenderSize.Width;
                verticalAxisX = Math.Round(temp * 2) / 2;
            }

            CallSymmetryDragCommand((SymmetryAxisDirection)capturedDirection, verticalAxisX);
        }
        e.Handled = true;
    }

    private static void OnPositionUpdate(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var self = (SymmetryOverlay)obj;
        self.horizontalAxisY = self.HorizontalAxisY;
        self.verticalAxisX = self.VerticalAxisX;
        self.InvalidateVisual();
    }
}
