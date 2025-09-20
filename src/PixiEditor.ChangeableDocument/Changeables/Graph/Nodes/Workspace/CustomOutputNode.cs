using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;

[NodeInfo("CustomOutput")]
public class CustomOutputNode : Node, IRenderInput
{
    public const string OutputNamePropertyName = "OutputName";
    public const string IsDefaultExportPropertyName = "IsDefaultExport";
    public const string SizePropertyName = "Size";
    public RenderInputProperty Input { get; }
    public InputProperty<string> OutputName { get; }
    public InputProperty<bool> IsDefaultExport { get; }
    public InputProperty<VecI> Size { get; }

    private VecI? lastDocumentSize;

    private TextureCache textureCache = new TextureCache();

    public CustomOutputNode()
    {
        Input = new RenderInputProperty(this, OutputNode.InputPropertyName, "BACKGROUND", null);
        AddInputProperty(Input);

        OutputName = CreateInput(OutputNamePropertyName, "OUTPUT_NAME", "");
        IsDefaultExport = CreateInput(IsDefaultExportPropertyName, "IS_DEFAULT_EXPORT", false);
        Size = CreateInput(SizePropertyName, "SIZE", VecI.Zero);
    }

    public override Node CreateCopy()
    {
        return new CustomOutputNode();
    }

    protected override void OnExecute(RenderContext context)
    {
        if (context.TargetOutput == OutputName.Value)
        {
            VecI targetSize = Size.Value.ShortestAxis <= 0
                ? context.RenderOutputSize
                : (VecI)(Size.Value * context.ChunkResolution.Multiplier());

            lastDocumentSize = targetSize;

            DrawingSurface targetSurface = context.RenderSurface;

            int saved = targetSurface.Canvas.Save();

            Input.Value?.Paint(context, targetSurface);

            targetSurface.Canvas.RestoreToCount(saved);

            if (targetSurface != context.RenderSurface)
            {
                context.RenderSurface.Canvas.DrawSurface(targetSurface, 0, 0);
            }
        }
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
