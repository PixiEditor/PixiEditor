using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Rendering;

public class SceneRenderer
{
    public IReadOnlyDocument Document { get; }
    
    private readonly Func<KeyFrameTime> getActiveFrameTime;
    
    public SceneRenderer(IReadOnlyDocument document, Func<KeyFrameTime> getActiveFrameTime)
    {
        Document = document;
        this.getActiveFrameTime = getActiveFrameTime;
    }

    public void RenderScene(DrawingSurface target)
    {
        using RenderContext ctx = new(target, getActiveFrameTime(), ChunkResolution.Full, Document.Size);
        Document.NodeGraph.Execute(ctx);
    }
}
