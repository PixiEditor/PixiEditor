using Avalonia.Media;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IBasicShapeToolbar : IBasicToolbar
{
    public bool Fill { get; }
    public Color FillColor { get; }
}
