using PixiEditor.Models.BrushEngine;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IPenToolbar : IToolbar, IToolSizeToolbar
{
    public bool AntiAliasing { get; set; }
    public float Spacing { get; set; }
    public PaintBrushShape PaintShape { get; set; }
    public Brush Brush { get; set; }
}
