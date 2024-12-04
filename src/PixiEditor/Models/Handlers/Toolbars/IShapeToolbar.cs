using Avalonia.Media;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IShapeToolbar : IToolSizeToolbar
{
    public Color StrokeColor { get; set; }
    public bool SyncWithPrimaryColor { get; set; }
    public bool AntiAliasing { get; set; }
}
