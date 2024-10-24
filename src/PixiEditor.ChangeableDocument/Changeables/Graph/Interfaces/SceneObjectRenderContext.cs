using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public class SceneObjectRenderContext : RenderContext
{
    public RectD LocalBounds { get; }
    public bool RenderSurfaceIsScene { get; }
    public RenderOutputProperty TargetPropertyOutput { get; }

    public SceneObjectRenderContext(RenderOutputProperty targetPropertyOutput, DrawingSurface surface, RectD localBounds, KeyFrameTime frameTime,
        ChunkResolution chunkResolution, VecI docSize, bool renderSurfaceIsScene, double opacity) : base(surface, frameTime, chunkResolution, docSize, opacity)
    {
        TargetPropertyOutput = targetPropertyOutput;
        LocalBounds = localBounds;
        RenderSurfaceIsScene = renderSurfaceIsScene;
    }
}
