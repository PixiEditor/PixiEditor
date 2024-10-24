using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IRasterizable
{
    public void Rasterize(DrawingSurface surface, Paint paint);
}
