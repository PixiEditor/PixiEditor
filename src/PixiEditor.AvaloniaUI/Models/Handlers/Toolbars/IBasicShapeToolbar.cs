using Avalonia.Media;

namespace PixiEditor.AvaloniaUI.Models.Handlers.Toolbars;

internal interface IBasicShapeToolbar : IBasicToolbar
{
    public bool Fill { get; }
    public Color FillColor { get; }
}
