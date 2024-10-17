using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Output")]
public class OutputNode : Node, IRenderInput, IPreviewRenderable
{
    public const string InputPropertyName = "Background";

    public RenderInputProperty Input { get; } 
    
    private VecI? lastDocumentSize;
    public OutputNode()
    {
        Input = new RenderInputProperty(this, InputPropertyName, "BACKGROUND", null);
        AddInputProperty(Input);
    }

    public override Node CreateCopy()
    {
        return new OutputNode();
    }

    protected override void OnExecute(RenderContext context)
    {
        lastDocumentSize = context.DocumentSize;
        Input.Value?.Paint(context, context.RenderSurface);
    }

    RenderInputProperty IRenderInput.Background => Input;
    public RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        if (lastDocumentSize == null)
        {
            return null;
        }
        
        return new RectD(0, 0, lastDocumentSize.Value.X, lastDocumentSize.Value.Y); 
    }

    public bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame, string elementToRenderName)
    {
        var executionQueue = GraphUtils.CalculateExecutionQueue(this);
        
        foreach (var node in executionQueue)
        {
            if(node == this)
            {
                continue;
            }
            
            if (node is IPreviewRenderable previewRenderable)
            {
                if (!previewRenderable.RenderPreview(renderOn, resolution, frame, elementToRenderName))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
