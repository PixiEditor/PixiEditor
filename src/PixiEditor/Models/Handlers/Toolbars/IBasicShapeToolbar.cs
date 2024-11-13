using Avalonia.Media;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IBasicShapeToolbar : IBasicToolbar
{
    public Color StrokeColor { get; set; }
    public bool Fill { get; set; }
    public Color FillColor { get; set; }
    public bool SyncWithPrimaryColor { get; set; }
    public bool AntiAliasing { get; set; }
}
