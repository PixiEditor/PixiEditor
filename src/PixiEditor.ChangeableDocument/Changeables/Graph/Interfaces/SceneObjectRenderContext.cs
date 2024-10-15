using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public class SceneObjectRenderContext : RenderContext
{
    public RectD LocalBounds { get; }
    public bool RenderSurfaceIsScene { get; }

    public SceneObjectRenderContext(DrawingSurface surface, RectD localBounds, KeyFrameTime frameTime,
        ChunkResolution chunkResolution, VecI docSize, bool renderSurfaceIsScene, double opacity) : base(surface, frameTime, chunkResolution, docSize, opacity)
    {
        LocalBounds = localBounds;
        RenderSurfaceIsScene = renderSurfaceIsScene;
    }
}
