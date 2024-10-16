using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class RenderNode : Node, IPreviewRenderable 
{
    public RenderOutputProperty Output { get; }
    
    public RenderNode()
    {
        Output = CreateRenderOutput("Output", "OUTPUT");
    }

    protected override void OnExecute(RenderContext context)
    {
        using RenderContext ctx = new RenderContext(
            Output.GetFirstRenderTarget(context), 
            context.FrameTime, context.ChunkResolution, context.DocumentSize, context.Opacity);
        ctx.FullRerender = context.FullRerender;
        
        Output.Value = ExecuteRender(ctx);
    }
    
    protected abstract DrawingSurface? ExecuteRender(RenderContext context);
    
    protected RenderOutputProperty? CreateRenderOutput(string internalName, string displayName)
    {
        RenderOutputProperty prop = new RenderOutputProperty(this, internalName, displayName, null);
        AddOutputProperty(prop);

        return prop;
    }

    protected RenderInputProperty CreateRenderInput(string internalName, string displayName,
        Func<RenderContext, DrawingSurface> renderTarget)
    {
        RenderInputProperty prop = new RenderInputProperty(this, internalName, displayName, null, renderTarget);
        AddInputProperty(prop);

        return prop;
    }

    public abstract RectD? GetPreviewBounds(int frame, string elementToRenderName = "");

    public abstract bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame,
        string elementToRenderName);

}
