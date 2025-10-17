using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IRasterizable
{
    public void Rasterize(DrawingSurface surface, Paint paint, int atFrame);
}
