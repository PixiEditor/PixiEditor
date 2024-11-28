using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class RenderNode : Node, IPreviewRenderable, IHighDpiRenderNode
{
    public RenderOutputProperty Output { get; }

    public bool AllowHighDpiRendering { get; set; } = false;

    public RenderNode()
    {
        Painter painter = new Painter(Paint);
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
    
    private void Paint(RenderContext context, DrawingSurface surface)
    {
        DrawingSurface target = surface;
        bool useIntermediate = !AllowHighDpiRendering 
                               && context.DocumentSize is { X: > 0, Y: > 0 } 
                               && surface.DeviceClipBounds.Size != context.DocumentSize;
        if (useIntermediate)
        {
            Texture intermediate = RequestTexture(0, context.DocumentSize);
            target = intermediate.DrawingSurface;
        }

        OnPaint(context, target);
        
        if(useIntermediate)
        {
            surface.Canvas.DrawSurface(target, 0, 0);
        }
    }

    protected abstract void OnPaint(RenderContext context, DrawingSurface surface);

    public abstract RectD? GetPreviewBounds(int frame, string elementToRenderName = "");

    public abstract bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame,
        string elementToRenderName);
}
