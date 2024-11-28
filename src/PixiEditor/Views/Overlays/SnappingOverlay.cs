using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.Models.Controllers.InputDevice;
using Drawie.Numerics;
using PixiEditor.Helpers;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;
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

    private Paint horizontalAxisPen;
    private Paint verticalAxisPen; 
    private Paint previewPointPen;

    private const float startSize = 1;
    
    static SnappingOverlay()
    {
        AffectsRender<SnappingOverlay>(SnappingControllerProperty);
        SnappingControllerProperty.Changed.Subscribe(SnappingControllerChanged);
    }
    
    public SnappingOverlay()
    {
        /*TODO: Theme variant is not present, that's why Dark is hardcoded*/        
        horizontalAxisPen = ResourceLoader.GetPaint("HorizontalSnapAxisBrush", PaintStyle.Stroke, ThemeVariant.Dark) ?? new Paint() { Color = Colors.Red, Style = PaintStyle.Stroke, IsAntiAliased = true, StrokeWidth = startSize};
        verticalAxisPen = ResourceLoader.GetPaint("VerticalSnapAxisBrush", PaintStyle.Stroke, ThemeVariant.Dark) ?? new Paint() { Color = Colors.Green, Style = PaintStyle.Stroke, IsAntiAliased = true, StrokeWidth = startSize}; 
        previewPointPen = ResourceLoader.GetPaint("SnapPointPreviewBrush", PaintStyle.Fill, ThemeVariant.Dark) ?? new Paint() { Color = Colors.Blue, Style = PaintStyle.Stroke, IsAntiAliased = true, StrokeWidth = startSize}; 
        IsHitTestVisible = false;
    }

    public override void RenderOverlay(Canvas context, RectD canvasBounds)
    {
        if (SnappingController is null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(SnappingController.HighlightedXAxis))
        {
            foreach (var snapPoint in SnappingController.HorizontalSnapPoints)
            {
                if (snapPoint.Key == SnappingController.HighlightedXAxis)
                {
                    context.DrawLine(new VecD(snapPoint.Value(), 0), new VecD(snapPoint.Value(), canvasBounds.Height), horizontalAxisPen);
                }
            }
        }
        
        if (!string.IsNullOrEmpty(SnappingController.HighlightedYAxis))
        {
            foreach (var snapPoint in SnappingController.VerticalSnapPoints)
            {
                if (snapPoint.Key == SnappingController.HighlightedYAxis)
                {
                    context.DrawLine(new VecD(0, snapPoint.Value()), new VecD(canvasBounds.Width, snapPoint.Value()), verticalAxisPen);
                }
            }
        }
        
        if (SnappingController.HighlightedPoint.HasValue)
        {
            context.DrawOval((float)SnappingController.HighlightedPoint.Value.X, (float)SnappingController.HighlightedPoint.Value.Y, 4f / (float)ZoomScale, 4f / (float)ZoomScale, previewPointPen);
        }
    }

    protected override void ZoomChanged(double newZoom)
    {
        horizontalAxisPen.StrokeWidth = startSize / (float)newZoom;
        verticalAxisPen.StrokeWidth = startSize / (float)newZoom;
        previewPointPen.StrokeWidth = startSize / (float)newZoom;
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
