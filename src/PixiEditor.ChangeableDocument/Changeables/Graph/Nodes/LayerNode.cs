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
        RenderPreviews(sceneContext);
        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = Background.Value;
            return;
        }

        blendPaint.Color = new Color(255, 255, 255, 255);
        blendPaint.BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.SrcOver;

        RenderContent(sceneContext, sceneContext.RenderSurface,
            sceneContext.TargetPropertyOutput == Output);
    }

    private void RenderContent(SceneObjectRenderContext context, Canvas renderOnto, bool useFilters)
    {
        if (renderOnto == null)
            return;

        if (!HasOperations())
        {
            if (Background.Value != null)
            {
                blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            }

            // TODO: Optimizattion: Simple graphs can draw directly to scene, skipping the intermediate surface
            if (AllowHighDpiRendering || renderOnto.DeviceClipBounds.Size == context.RenderOutputSize)
            {
                DrawLayerInScene(context, renderOnto, useFilters);
            }
            else
            {
                using var targetPaint = new Paint
                {
                    Color = new Color(255, 255, 255, 255),
                    BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.SrcOver
                };

                // Full because RenderOutputSize should already be in the correct resolution
                var tempSurface = TryInitWorkingSurface(context.RenderOutputSize, ChunkResolution.Full,
                    context.ProcessingColorSpace, 22);

                var originalSurface = context.RenderSurface;
                context.RenderSurface = tempSurface.DrawingSurface.Canvas;

                DrawLayerOnTexture(context, tempSurface.DrawingSurface.Canvas, context.ChunkResolution, useFilters,
                    targetPaint);

                context.RenderSurface = originalSurface;

                blendPaint.SetFilters(null);
                DrawWithResolution(tempSurface.DrawingSurface, renderOnto, context.ChunkResolution,
                    context.DesiredSamplingOptions);
            }

            return;
        }

        VecI size = AllowHighDpiRendering
            ? renderOnto.DeviceClipBounds.Size + renderOnto.DeviceClipBounds.Pos
            : context.RenderOutputSize;
        int saved = renderOnto.Save();

        var adjustedResolution = AllowHighDpiRendering ? ChunkResolution.Full : context.ChunkResolution;

        // Full because scene already handles texture resolution
        var outputWorkingSurface =
            TryInitWorkingSurface(size, ChunkResolution.Full, context.ProcessingColorSpace, 1);
        outputWorkingSurface.DrawingSurface.Canvas.Clear();
        outputWorkingSurface.DrawingSurface.Canvas.Save();
        if (AllowHighDpiRendering)
        {
            outputWorkingSurface.DrawingSurface.Canvas.SetMatrix(renderOnto.TotalMatrix);
            renderOnto.SetMatrix(Matrix3X3.Identity);
        }

        using var paint = new Paint
        {
            Color = new Color(255, 255, 255, 255), BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.SrcOver
        };


        var originalRenderSurface = context.RenderSurface;
        context.RenderSurface = outputWorkingSurface.DrawingSurface.Canvas;

        DrawLayerOnTexture(context, outputWorkingSurface.DrawingSurface.Canvas, adjustedResolution, false, paint);

        context.RenderSurface = originalRenderSurface;

        ApplyMaskIfPresent(outputWorkingSurface.DrawingSurface.Canvas, context, adjustedResolution);

        if (Background.Value != null)
        {
            Texture tempSurface = TryInitWorkingSurface(size, adjustedResolution, context.ProcessingColorSpace, 4);

            tempSurface.DrawingSurface.Canvas.Save();
            if (AllowHighDpiRendering)
            {
                tempSurface.DrawingSurface.Canvas.SetMatrix(outputWorkingSurface.DrawingSurface.Canvas.TotalMatrix);
                outputWorkingSurface.DrawingSurface.Canvas.SetMatrix(Matrix3X3.Identity);
            }
            else
            {
                tempSurface.DrawingSurface.Canvas.Scale(
                    (float)context.ChunkResolution.Multiplier());
            }

            tempSurface.DrawingSurface.Canvas.Clear();
            if (Background.Connection is { Node: IClipSource clipSource } && ClipToPreviousMember)
            {
                DrawClipSource(tempSurface.DrawingSurface.Canvas, clipSource, context);
            }

            ApplyRasterClip(outputWorkingSurface.DrawingSurface, tempSurface.DrawingSurface);
        }

        blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
        if (useFilters)
        {
            blendPaint.SetFilters(Filters.Value);
        }
        else
        {
            blendPaint.SetFilters(null);
        }

        DrawWithResolution(outputWorkingSurface.DrawingSurface, renderOnto, adjustedResolution,
            context.DesiredSamplingOptions);

        renderOnto.RestoreToCount(saved);
        outputWorkingSurface.DrawingSurface.Canvas.Restore();
    }

    protected internal virtual void DrawLayerOnTexture(SceneObjectRenderContext ctx,
        Canvas workingSurface,
        ChunkResolution resolution,
        bool useFilters, Paint paint)
    {
        int scaled = workingSurface.Save();
        workingSurface.Scale((float)resolution.Multiplier());

        DrawLayerOnto(ctx, workingSurface, useFilters, paint);

        workingSurface.RestoreToCount(scaled);
    }

    private void DrawWithResolution(DrawingSurface source, Canvas target, ChunkResolution resolution,
        SamplingOptions sampling)
    {
        int scaled = target.Save();
        float multiplier = (float)resolution.InvertedMultiplier();
        target.Scale(multiplier, multiplier);

        if (sampling == SamplingOptions.Default)
        {
            target.DrawSurface(source, 0, 0, blendPaint);
        }
        else
        {
            using var snapshot = source.Snapshot();
            target.DrawImage(snapshot, 0, 0, sampling, blendPaint);
        }

        target.RestoreToCount(scaled);
    }


    protected internal virtual void DrawLayerInScene(SceneObjectRenderContext ctx,
        Canvas workingSurface,
        bool useFilters = true)
    {
        DrawLayerOnto(ctx, workingSurface, useFilters, blendPaint);
    }

    protected void DrawLayerOnto(SceneObjectRenderContext ctx, Canvas workingSurface,
        bool useFilters, Paint paint)
    {
        paint.Color = paint.Color.WithAlpha((byte)Math.Round(Opacity.Value * ctx.Opacity * 255));

        var targetSurface = workingSurface;
        Texture? tex = null;
        int saved = -1;
        if (!ctx.ProcessingColorSpace.IsSrgb && ((useFilters && Filters.Value != null) || MustRenderInSrgb(ctx)))
        {
            saved = workingSurface.Save();

            tex = Texture.ForProcessing(workingSurface,
                ColorSpace.CreateSrgb());
            workingSurface.SetMatrix(Matrix3X3.Identity);
            ctx.RenderSurface = tex.DrawingSurface.Canvas;
            targetSurface = tex.DrawingSurface.Canvas;
        }

        if (useFilters && Filters.Value != null)
        {
            paint.SetFilters(Filters.Value);
            DrawWithFilters(ctx, targetSurface, paint);
        }
        else
        {
            paint.SetFilters(null);
            DrawWithoutFilters(ctx, targetSurface, paint);
        }

        if (targetSurface != workingSurface)
        {
            workingSurface.DrawSurface(targetSurface.Surface, 0, 0);
            tex.Dispose();
            workingSurface.RestoreToCount(saved);
            ctx.RenderSurface = workingSurface;
        }
    }

    protected virtual bool MustRenderInSrgb(SceneObjectRenderContext ctx)
    {
        return false;
    }

    protected abstract void DrawWithoutFilters(SceneObjectRenderContext ctx, Canvas workingSurface,
        Paint paint);

    protected abstract void DrawWithFilters(SceneObjectRenderContext ctx, Canvas workingSurface,
        Paint paint);

    protected Texture TryInitWorkingSurface(VecI imageSize, ChunkResolution resolution, ColorSpace processingCs, int id)
    {
        ChunkResolution targetResolution = resolution;
        bool hasSurface = workingSurfaces.TryGetValue((targetResolution, id), out Texture workingSurface);
        VecI targetSize = (VecI)(imageSize * targetResolution.Multiplier());

        targetSize = new VecI(Math.Max(1, targetSize.X), Math.Max(1, targetSize.Y));

        if (!hasSurface || workingSurface.Size != targetSize || workingSurface.IsDisposed)
        {
            workingSurface?.Dispose();
            workingSurfaces[(targetResolution, id)] = Texture.ForProcessing(targetSize, processingCs);
            workingSurface = workingSurfaces[(targetResolution, id)];
        }
        else
        {
            workingSurface.DrawingSurface.Canvas.SetMatrix(Matrix3X3.Identity);
            workingSurface.DrawingSurface.Canvas.Clear();
        }

        return workingSurface;
    }

    void IClipSource.DrawClipSource(SceneObjectRenderContext context, Canvas drawOnto)
    {
        RenderContent(context, drawOnto, false);
    }

    public override void Dispose()
    {
        base.Dispose();
        if (workingSurfaces != null)
        {
            foreach (var workingSurface in workingSurfaces.Values)
            {
                workingSurface?.Dispose();
            }
        }
    }
}
