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
        Painter painter = new Painter(OnPaint);
        Output = CreateRenderOutput("Output", "OUTPUT", 
            () => painter, 
            () => this is IRenderInput renderInput ? renderInput.Background.Value : null);
    }

    protected override void OnExecute(RenderContext context)
    {
        foreach (var prop in OutputProperties)
        {
            if (prop is RenderOutputProperty output)
            {
                output.ChainToPainterValue();
            }
        } 
    }
    
    protected abstract void OnPaint(RenderContext context, DrawingSurface surface);
    
    protected RenderOutputProperty? CreateRenderOutput(string internalName, string displayName, Func<Painter?>? nextInChain, Func<Painter?>? previous = null)
    {
        RenderOutputProperty prop = new RenderOutputProperty(this, internalName, displayName, null);
        prop.FirstInChain = previous;
        prop.NextInChain = nextInChain;
        AddOutputProperty(prop);

        return prop;
    }

    protected RenderInputProperty CreateRenderInput(string internalName, string displayName)
    {
        RenderInputProperty prop = new RenderInputProperty(this, internalName, displayName, null);
        AddInputProperty(prop);

        return prop;
    }

    public abstract RectD? GetPreviewBounds(int frame, string elementToRenderName = "");

    public abstract bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame,
        string elementToRenderName);

}
