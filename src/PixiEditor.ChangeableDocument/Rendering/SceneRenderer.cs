using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Rendering;

public class SceneRenderer
{
    public IReadOnlyDocument Document { get; }
    public ChunkResolution Resolution { get; set; }
    public HashSet<VecI> VisibleChunks { get; set; }

    private readonly Func<KeyFrameTime> getActiveFrameTime;
    
    public SceneRenderer(IReadOnlyDocument document, Func<KeyFrameTime> getActiveFrameTime)
    {
        Document = document;
        this.getActiveFrameTime = getActiveFrameTime;
    }

    public void RenderScene(DrawingSurface target)
    {
        using RenderContext ctx = new(target, getActiveFrameTime(), Resolution, Document.Size) { VisibleChunks = this.VisibleChunks};
        Document.NodeGraph.Execute(ctx);
    }
}
