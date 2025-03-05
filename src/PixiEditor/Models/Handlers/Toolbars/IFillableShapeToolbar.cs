using Avalonia.Media;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;

namespace PixiEditor.Models.Handlers.Toolbars;

internal interface IFillableShapeToolbar : IShapeToolbar
{
    public bool Fill { get; set; }
    public IBrush FillBrush { get; set; }
}
