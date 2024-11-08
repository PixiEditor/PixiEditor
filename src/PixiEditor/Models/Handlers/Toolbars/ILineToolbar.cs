using Avalonia.Media;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface ILineToolbar : IBasicToolbar
{
    public Color StrokeColor { get; set; }
    public bool AntiAliasing { get; set; }
}
