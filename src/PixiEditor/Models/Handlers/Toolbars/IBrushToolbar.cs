using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IBrushToolbar : IToolbar, IToolSizeToolbar
{
    public bool AntiAliasing { get; set; }
    public Brush Brush { get; set; }
    public BrushData CreateBrushData();
    public BrushData LastBrushData { get; }
}
