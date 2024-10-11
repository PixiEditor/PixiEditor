using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IClipSource
{
    public void DrawOnTexture(SceneObjectRenderContext context, DrawingSurface drawOnto);
}
