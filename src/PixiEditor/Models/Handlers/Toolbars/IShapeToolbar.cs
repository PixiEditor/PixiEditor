using Avalonia.Media;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IShapeToolbar : IToolSizeToolbar
{
    public IBrush StrokeBrush { get; set; }
    public bool SyncWithPrimaryColor { get; set; }
    public bool AntiAliasing { get; set; }
}
