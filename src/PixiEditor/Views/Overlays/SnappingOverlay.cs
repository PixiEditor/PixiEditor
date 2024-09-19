using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Numerics;
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
    
    public SnappingOverlay()
    {
        /*TODO: Theme variant is not present, that's why Dark is hardcoded*/        
        horizontalAxisPen = Application.Current.Styles.TryGetResource("HorizontalSnapAxisBrush", ThemeVariant.Dark, out var horizontalAxisBrush) ? new Pen((IBrush)horizontalAxisBrush, 0.2f) : new Pen(Brushes.Red, 0.2f);
        verticalAxisPen = Application.Current.Styles.TryGetResource("VerticalSnapAxisBrush", ThemeVariant.Dark, out var verticalAxisBrush) ? new Pen((IBrush)verticalAxisBrush, 0.2f) : new Pen(Brushes.Green, 0.2f);
    }

    public override void RenderOverlay(DrawingContext context, RectD canvasBounds)
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
                    context.DrawLine(horizontalAxisPen, new Point(snapPoint.Value, 0), new Point(snapPoint.Value, canvasBounds.Height));
                }
            }
        }
        
        if (!string.IsNullOrEmpty(SnappingController.HighlightedYAxis))
        {
            foreach (var snapPoint in SnappingController.VerticalSnapPoints)
            {
                if (snapPoint.Key == SnappingController.HighlightedYAxis)
                {
                    context.DrawLine(verticalAxisPen, new Point(0, snapPoint.Value), new Point(canvasBounds.Width, snapPoint.Value));
                }
            }
        }
    }
}
