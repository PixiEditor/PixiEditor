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
    public const string UniqueName = "PixiEditor.Output";
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
        if (!string.IsNullOrEmpty(context.TargetOutput)) return;

        lastDocumentSize = context.RenderOutputSize;

        int saved = context.RenderSurface.Canvas.Save();
        context.RenderSurface.Canvas.ClipRect(new RectD(0, 0, context.RenderOutputSize.X, context.RenderOutputSize.Y));
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
