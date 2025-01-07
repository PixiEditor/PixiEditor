using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IPreviewRenderable
{
    public Dictionary<int, Image> LastRenderedPreviews { get; }
    public Guid RenderableId { get; }
    public RectD? GetPreviewBounds(int frame, string elementToRenderName = ""); 
    public bool RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName);
}
