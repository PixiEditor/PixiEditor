using System.ComponentModel;

namespace PixiEditor.Views.Overlays.BrushShapeOverlay;
internal enum BrushShape
{
    [Description("BRUSH_SHAPE_HIDDEN")]
    Hidden,
    [Description("BRUSH_SHAPE_PIXEL")]
    Pixel,
    [Description("BRUSH_SHAPE_SQUARE")]
    Square,
    [Description("BRUSH_SHAPE_CIRCLE_PIXELATED")]
    CirclePixelated,
    [Description("BRUSH_SHAPE_CIRCLE_SMOOTH")]
    CircleSmooth
}
