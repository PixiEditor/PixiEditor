using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IPreviewRenderable
{
    public bool RenderPreview(Texture renderOn, VecI chunk, ChunkResolution resolution, int frame);
}
