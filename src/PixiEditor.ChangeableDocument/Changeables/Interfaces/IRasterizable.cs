using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IRasterizable
{
    public void Rasterize(DrawingSurface surface, ChunkResolution resolution, Paint paint);
}
