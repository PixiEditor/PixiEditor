using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Models.Controllers.InputDevice;
using Drawie.Numerics;
using Point = Avalonia.Point;

namespace PixiEditor.Views.Overlays;

internal class SnappingOverlay : Overlay
{
    public static readonly StyledProperty<SnappingController?> SnappingControllerProperty = AvaloniaProperty.Register<SnappingOverlay, SnappingController?>(
        nameof(SnappingController));

    public SnappingController? SnappingController
    {
        get => GetValue(SnappingControllerProperty);
        set => SetValue(SnappingControllerProperty, value);
    }

    private Pen horizontalAxisPen;
    private Pen verticalAxisPen; 
    private Pen previewPointPen;

    private const double startSize = 2;
    
    static SnappingOverlay()
    {
        AffectsRender<SnappingOverlay>(SnappingControllerProperty);
        SnappingControllerProperty.Changed.Subscribe(SnappingControllerChanged);
    }
    
    public SnappingOverlay()
    {
        /*TODO: Theme variant is not present, that's why Dark is hardcoded*/        
        horizontalAxisPen = Application.Current.Styles.TryGetResource("HorizontalSnapAxisBrush", ThemeVariant.Dark, out var horizontalAxisBrush) ? new Pen((IBrush)horizontalAxisBrush, startSize) : new Pen(Brushes.Red, startSize);
        verticalAxisPen = Application.Current.Styles.TryGetResource("VerticalSnapAxisBrush", ThemeVariant.Dark, out var verticalAxisBrush) ? new Pen((IBrush)verticalAxisBrush, startSize) : new Pen(Brushes.Green, startSize);
        previewPointPen = Application.Current.Styles.TryGetResource("SnapPointPreviewBrush", ThemeVariant.Dark, out var previewPointBrush) ? new Pen((IBrush)previewPointBrush, startSize) : new Pen(Brushes.DodgerBlue, startSize);
        IsHitTestVisible = false;
    }

    public override void RenderOverlay(Canvas context, RectD canvasBounds)
    {
        /*if (SnappingController is null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(SnappingController.HighlightedXAxis))
        {
            foreach (var snapPoint in SnappingController.HorizontalSnapPoints)
            {
                if (snapPoint.Key == SnappingController.HighlightedXAxis)
                {
                    context.DrawLine(horizontalAxisPen, new Point(snapPoint.Value(), 0), new Point(snapPoint.Value(), canvasBounds.Height));
                }
            }
        }
        
        if (!string.IsNullOrEmpty(SnappingController.HighlightedYAxis))
        {
            foreach (var snapPoint in SnappingController.VerticalSnapPoints)
            {
                if (snapPoint.Key == SnappingController.HighlightedYAxis)
                {
                    context.DrawLine(verticalAxisPen, new Point(0, snapPoint.Value()), new Point(canvasBounds.Width, snapPoint.Value()));
                }
            }
        }
        
        if (SnappingController.HighlightedPoint.HasValue)
        {
            context.DrawEllipse(previewPointPen.Brush, previewPointPen, new Point(SnappingController.HighlightedPoint.Value.X, SnappingController.HighlightedPoint.Value.Y), 5 / ZoomScale, 5 / ZoomScale);
        }*/
    }

    protected override void ZoomChanged(double newZoom)
    {
        horizontalAxisPen.Thickness = startSize / newZoom;
        verticalAxisPen.Thickness = startSize / newZoom;
        previewPointPen.Thickness = startSize / newZoom;
    }

    private static void SnappingControllerChanged(AvaloniaPropertyChangedEventArgs e)
    {
        SnappingOverlay overlay = (SnappingOverlay)e.Sender;
        if (e.OldValue is SnappingController oldSnappingController)
        {
            oldSnappingController.HorizontalHighlightChanged -= overlay.SnapAxisChanged;
            oldSnappingController.VerticalHighlightChanged -= overlay.SnapAxisChanged;
            oldSnappingController.HighlightedPointChanged -= overlay.OnHighlightedPointChanged;
        }
        if (e.NewValue is SnappingController snappingController)
        {
            snappingController.HorizontalHighlightChanged += overlay.SnapAxisChanged;
            snappingController.VerticalHighlightChanged += overlay.SnapAxisChanged;
            snappingController.HighlightedPointChanged += overlay.OnHighlightedPointChanged;
        }
    }

    private void SnapAxisChanged(string axis)
    {
        Refresh();
    }
    
    private void OnHighlightedPointChanged(VecD? point)
    {
        Refresh();
    }
}
