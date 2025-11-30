using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IRasterizable
{
    public void Rasterize(Canvas surface, Paint paint, int atFrame);
}
