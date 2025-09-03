using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Output")]
public class OutputNode : Node, IRenderInput
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

        Input.Value?.Paint(context, context.RenderSurface);
        lastDocumentSize = context.DocumentSize;

        var previews = context.GetPreviewTexturesForNode(Id);
        if (previews is null) return;
        foreach (var request in previews)
        {
            var texture = request.Texture;
            if (texture is null) continue;

            int saved = texture.DrawingSurface.Canvas.Save();
            texture.DrawingSurface.Canvas.Scale((float)context.ChunkResolution.Multiplier());
            Input.Value?.Paint(context, texture.DrawingSurface);
            texture.DrawingSurface.Canvas.RestoreToCount(saved);
        }
    }

    RenderInputProperty IRenderInput.Background => Input;
}
