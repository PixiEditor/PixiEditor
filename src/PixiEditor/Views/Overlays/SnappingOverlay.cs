using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using PixiEditor.Models.Controllers.InputDevice;
using Drawie.Numerics;
using PixiEditor.Extensions.UI.Overlays;
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

    private Paint distanceTextPaint;
    private Font distanceFont = Font.CreateDefault();

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
        distanceTextPaint = new Paint() { Color = Colors.White, Style = PaintStyle.Fill, IsAntiAliased = true, StrokeWidth = startSize };

        IsHitTestVisible = false;
    }

    public override void RenderOverlay(Canvas context, RectD canvasBounds)
    {
        if (SnappingController is null)
        {
            return;
        }

        VecD mousePoint = SnappingController.HighlightedPoint ?? PointerPosition;

        if (!string.IsNullOrEmpty(SnappingController.HighlightedXAxis))
        {
            foreach (var snapPoint in SnappingController.HorizontalSnapPoints)
            {
                if (snapPoint.Key == SnappingController.HighlightedXAxis)
                {
                    VecD snapPointValue = snapPoint.Value();
                    context.DrawLine(new VecD(snapPointValue.X, mousePoint.Y), new VecD(snapPointValue.X, snapPointValue.Y), horizontalAxisPen);

                    DrawDistanceText(context, snapPointValue + new VecD(10 / ZoomScale, 0), mousePoint);
                }
            }
        }
        
        if (!string.IsNullOrEmpty(SnappingController.HighlightedYAxis))
        {
            foreach (var snapPoint in SnappingController.VerticalSnapPoints)
            {
                if (snapPoint.Key == SnappingController.HighlightedYAxis)
                {
                    var snapPointValue = snapPoint.Value();
                    context.DrawLine(new VecD(mousePoint.X, snapPointValue.Y), new VecD(snapPointValue.X, snapPointValue.Y), verticalAxisPen);

                    DrawDistanceText(context, snapPointValue + new VecD(0, -10 / ZoomScale), mousePoint);
                }
            }
        }
        
        if (SnappingController.HighlightedPoint.HasValue)
        {
            context.DrawOval((float)SnappingController.HighlightedPoint.Value.X, (float)SnappingController.HighlightedPoint.Value.Y, 4f / (float)ZoomScale, 4f / (float)ZoomScale, previewPointPen);
        }
    }

    private void DrawDistanceText(Canvas context, VecD snapPointValue, VecD mousePoint)
    {
        VecD distance = snapPointValue - mousePoint;
        VecD center = (snapPointValue + mousePoint) / 2;
        distanceFont.Size = 12 / (float)ZoomScale;

        distanceTextPaint.Color = Colors.Black;
        distanceTextPaint.Style = PaintStyle.Stroke;
        distanceTextPaint.StrokeWidth = 2f / (float)ZoomScale;

        context.DrawText($"{distance.Length.ToString("F2", CultureInfo.CurrentCulture)} px", center, distanceFont, distanceTextPaint);
        distanceTextPaint.Color = Colors.White;
        distanceTextPaint.Style = PaintStyle.Fill;
        context.DrawText($"{distance.Length.ToString("F2", CultureInfo.CurrentCulture)} px", center, distanceFont, distanceTextPaint);
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
