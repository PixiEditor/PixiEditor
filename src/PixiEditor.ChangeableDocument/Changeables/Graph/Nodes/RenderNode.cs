using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes.Structure;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class RenderNode : Node, IHighDpiRenderNode
{
    public RenderOutputProperty Output { get; }

    public bool AllowHighDpiRendering { get; set; } = false;

    public bool RendersInAbsoluteCoordinates { get; set; } = false;

    private TextureCache textureCache = new();

    private VecI lastDocumentSize = VecI.Zero;

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

        lastDocumentSize = context.DocumentSize;
    }

    protected virtual void Paint(RenderContext context, DrawingSurface surface)
    {
        DrawingSurface target = surface;
        bool useIntermediate = !AllowHighDpiRendering
                               && context.RenderOutputSize is { X: > 0, Y: > 0 }
                               && (surface.DeviceClipBounds.Size != context.RenderOutputSize ||
                                   (RendersInAbsoluteCoordinates && !surface.Canvas.TotalMatrix.IsIdentity));
        if (useIntermediate)
        {
            Texture intermediate =
                textureCache.RequestTexture(-6451, context.RenderOutputSize, context.ProcessingColorSpace);
            target = intermediate.DrawingSurface;
        }

        OnPaint(context, target);

        if (useIntermediate)
        {
            if (RendersInAbsoluteCoordinates)
            {
                surface.Canvas.Save();
                surface.Canvas.Scale((float)context.ChunkResolution.InvertedMultiplier());
            }

            if (context.DesiredSamplingOptions != SamplingOptions.Default)
            {
                using var snapshot = target.Snapshot();
                surface.Canvas.DrawImage(snapshot, 0, 0, context.DesiredSamplingOptions);
            }
            else
            {
                surface.Canvas.DrawSurface(target, 0, 0);
            }

            if (RendersInAbsoluteCoordinates)
            {
                surface.Canvas.Restore();
            }
        }

        RenderPreviews(context);
    }

    protected abstract void OnPaint(RenderContext context, DrawingSurface surface);

    protected void RenderPreviews(RenderContext ctx)
    {
        var previewToRender = ctx.GetPreviewTexturesForNode(Id);
        if (previewToRender == null || previewToRender.Count == 0)
            return;

        foreach (var preview in previewToRender)
        {
            if (!ShouldRenderPreview(preview.ElementToRender))
                continue;

            if (preview.Texture == null)
                continue;

            int saved = preview.Texture.DrawingSurface.Canvas.Save();
            preview.Texture.DrawingSurface.Canvas.Clear();

            var bounds = GetPreviewBounds(ctx, preview.ElementToRender);
            if (bounds == null)
            {
                bounds = new RectD(0, 0, ctx.RenderOutputSize.X, ctx.RenderOutputSize.Y);
            }

            VecD scaling = PreviewUtility.CalculateUniformScaling(bounds.Value.Size, preview.Texture.Size);
            VecD offset = PreviewUtility.CalculateCenteringOffset(bounds.Value.Size, preview.Texture.Size, scaling);
            RenderContext adjusted =
                PreviewUtility.CreatePreviewContext(ctx, scaling, bounds.Value.Size, preview.Texture.Size);

            preview.Texture.DrawingSurface.Canvas.Translate((float)offset.X, (float)offset.Y);
            preview.Texture.DrawingSurface.Canvas.Scale((float)scaling.X, (float)scaling.Y);
            preview.Texture.DrawingSurface.Canvas.Translate((float)-bounds.Value.X, (float)-bounds.Value.Y);

            adjusted.RenderSurface = preview.Texture.DrawingSurface;
            RenderPreview(preview.Texture.DrawingSurface, adjusted, preview.ElementToRender);
            preview.Texture.DrawingSurface.Canvas.RestoreToCount(saved);
        }
    }

    protected virtual bool ShouldRenderPreview(string elementToRenderName)
    {
        return true;
    }

    public virtual RectD? GetPreviewBounds(RenderContext ctx, string elementToRenderName)
    {
        return null;
    }

    public virtual void RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName)
    {
        int saved = renderOn.Canvas.Save();
        OnPaint(context, renderOn);
        renderOn.Canvas.RestoreToCount(saved);
    }

    protected Texture RequestTexture(int id, VecI size, ColorSpace processingCs, bool clear = true)
    {
        return textureCache.RequestTexture(id, size, processingCs, clear);
    }

    public override void SerializeAdditionalData(Dictionary<string, object> additionalData)
    {
        base.SerializeAdditionalData(additionalData);
        additionalData["AllowHighDpiRendering"] = AllowHighDpiRendering;
    }

    internal override void DeserializeAdditionalData(IReadOnlyDocument target, IReadOnlyDictionary<string, object> data,
        List<IChangeInfo> infos)
    {
        base.DeserializeAdditionalData(target, data, infos);

        if (data.TryGetValue("AllowHighDpiRendering", out var value))
            AllowHighDpiRendering = (bool)value;
    }

    public override void Dispose()
    {
        base.Dispose();
        textureCache.Dispose();
    }
}
