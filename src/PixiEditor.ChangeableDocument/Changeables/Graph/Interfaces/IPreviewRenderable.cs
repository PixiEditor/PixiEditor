using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IPreviewRenderable
{
    public RectD? GetPreviewBounds(int frame, string elementToRenderName = "");
    public bool RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName);
}
