using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public class SceneObjectRenderContext : RenderContext
{
    public RectD LocalBounds { get; }

    public SceneObjectRenderContext(DrawingSurface surface, RectD localBounds, KeyFrameTime frameTime, VecI chunkToUpdate, ChunkResolution chunkResolution, VecI docSize) : base(surface, frameTime, chunkToUpdate, chunkResolution, docSize)
    {
        LocalBounds = localBounds;
    }
}
