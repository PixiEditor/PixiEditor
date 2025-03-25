using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class LayerNode : StructureNode, IReadOnlyLayerNode, IClipSource
{
    protected Dictionary<(ChunkResolution, int), Texture> workingSurfaces =
        new Dictionary<(ChunkResolution, int), Texture>();

    public LayerNode()
    {
    }

    public override void Render(SceneObjectRenderContext sceneContext)
    {
        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = Background.Value;
            return;
        }

        blendPaint.Color = new Color(255, 255, 255, 255);
        blendPaint.BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.SrcOver;

        RenderContent(sceneContext, sceneContext.RenderSurface,
            sceneContext.TargetPropertyOutput != FilterlessOutput);
    }

    private void RenderContent(SceneObjectRenderContext context, DrawingSurface renderOnto, bool useFilters)
    {
        if (!HasOperations())
        {
            if (Background.Value != null)
            {
                blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            }

            DrawLayerInScene(context, renderOnto, useFilters);
            return;
        }

        VecI size = renderOnto.DeviceClipBounds.Size + renderOnto.DeviceClipBounds.Pos;
        int saved = renderOnto.Canvas.Save();

        var outputWorkingSurface =
            TryInitWorkingSurface(size, context.ChunkResolution, context.ProcessingColorSpace, 1);
        outputWorkingSurface.DrawingSurface.Canvas.Clear();
        outputWorkingSurface.DrawingSurface.Canvas.Save();
        outputWorkingSurface.DrawingSurface.Canvas.SetMatrix(renderOnto.Canvas.TotalMatrix);

        renderOnto.Canvas.SetMatrix(Matrix3X3.Identity);

        DrawLayerOnTexture(context, outputWorkingSurface.DrawingSurface, useFilters);

        ApplyMaskIfPresent(outputWorkingSurface.DrawingSurface, context);

        if (Background.Value != null)
        {
            Texture tempSurface = TryInitWorkingSurface(size, context.ChunkResolution, context.ProcessingColorSpace, 4);

            tempSurface.DrawingSurface.Canvas.Save();
            tempSurface.DrawingSurface.Canvas.SetMatrix(outputWorkingSurface.DrawingSurface.Canvas.TotalMatrix);

            outputWorkingSurface.DrawingSurface.Canvas.SetMatrix(Matrix3X3.Identity);

            tempSurface.DrawingSurface.Canvas.Clear();
            if (Background.Connection is { Node: IClipSource clipSource } && ClipToPreviousMember)
            {
                DrawClipSource(tempSurface.DrawingSurface, clipSource, context);
            }

            ApplyRasterClip(outputWorkingSurface.DrawingSurface, tempSurface.DrawingSurface);
        }

        blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
        DrawWithResolution(outputWorkingSurface.DrawingSurface, renderOnto, context.ChunkResolution);

        renderOnto.Canvas.RestoreToCount(saved);
        outputWorkingSurface.DrawingSurface.Canvas.Restore();
    }

    protected internal virtual void DrawLayerOnTexture(SceneObjectRenderContext ctx,
        DrawingSurface workingSurface,
        bool useFilters)
    {
        int scaled = workingSurface.Canvas.Save();
        workingSurface.Canvas.Scale((float)ctx.ChunkResolution.Multiplier());

        DrawLayerOnto(ctx, workingSurface, useFilters);

        workingSurface.Canvas.RestoreToCount(scaled);
    }

    private void DrawWithResolution(DrawingSurface source, DrawingSurface target, ChunkResolution resolution)
    {
        int scaled = target.Canvas.Save();
        float multiplier = (float)resolution.InvertedMultiplier();
        target.Canvas.Scale(multiplier, multiplier);

        target.Canvas.DrawSurface(source, 0, 0, blendPaint);

        target.Canvas.RestoreToCount(scaled);
    }

    protected abstract VecI GetTargetSize(RenderContext ctx);

    protected internal virtual void DrawLayerInScene(SceneObjectRenderContext ctx,
        DrawingSurface workingSurface,
        bool useFilters = true)
    {
        DrawLayerOnto(ctx, workingSurface, useFilters);
    }

    protected void DrawLayerOnto(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        bool useFilters)
    {
        blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * ctx.Opacity * 255));

        if (useFilters && Filters.Value != null)
        {
            blendPaint.SetFilters(Filters.Value);

            var targetSurface = workingSurface;
            Texture? tex = null;
            int saved = -1;
            if (!ctx.ProcessingColorSpace.IsSrgb)
            {
                saved = workingSurface.Canvas.Save();

                tex = Texture.ForProcessing(workingSurface, ColorSpace.CreateSrgb()); // filters are meant to be applied in sRGB
                targetSurface = tex.DrawingSurface;
                workingSurface.Canvas.SetMatrix(Matrix3X3.Identity);
            }

            DrawWithFilters(ctx, targetSurface, blendPaint);

            if(targetSurface != workingSurface)
            {
                workingSurface.Canvas.DrawSurface(targetSurface, 0, 0);
                tex.Dispose();
                workingSurface.Canvas.RestoreToCount(saved);
            }
        }
        else
        {
            blendPaint.SetFilters(null);
            DrawWithoutFilters(ctx, workingSurface, blendPaint);
        }
    }

    protected abstract void DrawWithoutFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        Paint paint);

    protected abstract void DrawWithFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        Paint paint);

    protected Texture TryInitWorkingSurface(VecI imageSize, ChunkResolution resolution, ColorSpace processingCs, int id)
    {
        ChunkResolution targetResolution = resolution;
        bool hasSurface = workingSurfaces.TryGetValue((targetResolution, id), out Texture workingSurface);
        VecI targetSize = (VecI)(imageSize * targetResolution.Multiplier());

        targetSize = new VecI(Math.Max(1, targetSize.X), Math.Max(1, targetSize.Y));

        if (!hasSurface || workingSurface.Size != targetSize || workingSurface.IsDisposed)
        {
            workingSurfaces[(targetResolution, id)] = Texture.ForProcessing(targetSize, processingCs);
            workingSurface = workingSurfaces[(targetResolution, id)];
        }

        return workingSurface;
    }

    void IClipSource.DrawClipSource(SceneObjectRenderContext context, DrawingSurface drawOnto)
    {
        RenderContent(context, drawOnto, false);
    }
}
