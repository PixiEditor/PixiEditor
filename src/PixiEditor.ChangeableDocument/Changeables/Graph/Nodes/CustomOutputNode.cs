using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("CustomOutput")]
public class CustomOutputNode : Node, IRenderInput, IPreviewRenderable
{
    public RenderInputProperty Input { get; } 
    public InputProperty<string> OutputName { get; }
    
    private VecI? lastDocumentSize;
    public CustomOutputNode()
    {
        Input = new RenderInputProperty(this, OutputNode.InputPropertyName, "BACKGROUND", null);
        AddInputProperty(Input);
        
        OutputName = CreateInput("OutputName", "OUTPUT_NAME", "");
    }

    public override Node CreateCopy()
    {
        return new CustomOutputNode();
    }

    protected override void OnExecute(RenderContext context)
    {
        lastDocumentSize = context.DocumentSize;
        
        int saved = context.RenderSurface.Canvas.Save();
        context.RenderSurface.Canvas.ClipRect(new RectD(0, 0, context.DocumentSize.X, context.DocumentSize.Y));
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

    public bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (Input.Value == null)
        {
            return false;
        }
        
        int saved = renderOn.Canvas.Save();
        Input.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);
        
        return true;
    }
}
