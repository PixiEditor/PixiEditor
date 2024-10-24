using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

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
        
        int saved = context.RenderSurface.Canvas.Save();
        Input.Value?.Paint(context, context.RenderSurface);
        
        context.RenderSurface.Canvas.RestoreToCount(saved);
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
        if (Input.Value == null)
        {
            return false;
        }
        
        RenderContext context = new(renderOn, frame, resolution, VecI.One);
        int saved = renderOn.Canvas.Save();
        Input.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);
        
        return true;
    }
}
