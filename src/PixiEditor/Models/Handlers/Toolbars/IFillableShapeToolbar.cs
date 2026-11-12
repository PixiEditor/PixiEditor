using Avalonia.Media;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IFillableShapeToolbar : IShapeToolbar
{
    public bool Fill { get; set; }
    public IBrush FillBrush { get; set; }
}
