using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IRasterizable
{
    public void Rasterize(DrawingSurface surface, ChunkResolution resolution);
}
