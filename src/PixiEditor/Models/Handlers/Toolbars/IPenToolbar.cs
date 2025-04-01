using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IPenToolbar : IToolbar, IToolSizeToolbar
{
    public bool AntiAliasing { get; set; }
    public float Hardness { get; set; }
    public float Spacing { get; set; }
    public PenBrushShape PenShape { get; set; }
}
