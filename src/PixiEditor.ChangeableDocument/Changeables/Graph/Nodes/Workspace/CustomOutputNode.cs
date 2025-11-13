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
    public const string FullViewportRenderPropertyName = "FullViewportRender";
    public RenderInputProperty Input { get; }
    public InputProperty<string> OutputName { get; }
    public InputProperty<bool> IsDefaultExport { get; }
    public InputProperty<VecI> Size { get; }
    public InputProperty<bool> FullViewportRender { get; }

    public CustomOutputNode()
    {
        Input = new RenderInputProperty(this, OutputNode.InputPropertyName, "BACKGROUND", null);
        AddInputProperty(Input);

        OutputName = CreateInput(OutputNamePropertyName, "OUTPUT_NAME", "");
        IsDefaultExport = CreateInput(IsDefaultExportPropertyName, "IS_DEFAULT_EXPORT", false);
        Size = CreateInput(SizePropertyName, "SIZE", VecI.Zero);
        FullViewportRender = CreateInput(FullViewportRenderPropertyName, "FULL_VIEWPORT_RENDER", false);
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

            Canvas targetSurface = context.RenderSurface;

            int saved = targetSurface.Save();

            Input.Value?.Paint(context, targetSurface);

            targetSurface.RestoreToCount(saved);

            if (targetSurface != context.RenderSurface)
            {
                context.RenderSurface.DrawSurface(targetSurface.Surface, 0, 0);
            }

            RenderPreviews(context);
        }
    }

    RenderInputProperty IRenderInput.Background => Input;

    protected void RenderPreviews(RenderContext ctx)
    {
        var previewToRender = ctx.GetPreviewTexturesForNode(Id);
        if (previewToRender == null || previewToRender.Count == 0)
            return;

        foreach (var preview in previewToRender)
        {
            if (preview.Texture == null)
                continue;

            int saved = preview.Texture.DrawingSurface.Canvas.Save();
            preview.Texture.DrawingSurface.Canvas.Clear();

            var bounds = new RectD(0, 0, ctx.RenderOutputSize.X, ctx.RenderOutputSize.Y);

            VecD scaling = PreviewUtility.CalculateUniformScaling(bounds.Size, preview.Texture.Size);
            VecD offset = PreviewUtility.CalculateCenteringOffset(bounds.Size, preview.Texture.Size, scaling);
            RenderContext adjusted =
                PreviewUtility.CreatePreviewContext(ctx, scaling, bounds.Size, preview.Texture.Size);

            preview.Texture.DrawingSurface.Canvas.Translate((float)offset.X, (float)offset.Y);
            preview.Texture.DrawingSurface.Canvas.Scale((float)scaling.X, (float)scaling.Y);
            preview.Texture.DrawingSurface.Canvas.Translate((float)-bounds.X, (float)-bounds.Y);

            adjusted.RenderSurface = preview.Texture.DrawingSurface.Canvas;
            RenderPreview(preview.Texture.DrawingSurface.Canvas, adjusted);
            preview.Texture.DrawingSurface.Canvas.RestoreToCount(saved);
        }
    }

    protected virtual void RenderPreview(Canvas surface, RenderContext context)
    {
        Input.Value?.Paint(context, surface);
    }
}
