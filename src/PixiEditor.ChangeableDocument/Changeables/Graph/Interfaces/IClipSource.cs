using Drawie.Backend.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IClipSource
{
    public void DrawClipSource(SceneObjectRenderContext context, DrawingSurface drawOnto);
}
