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
            sceneContext.TargetPropertyOutput == Output);
    }

    private void RenderContent(SceneObjectRenderContext context, DrawingSurface renderOnto, bool useFilters)
    {
        if (!HasOperations())
        {
            if (Background.Value != null)
            {
                blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            }

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

                var tempSurface = TryInitWorkingSurface(context.RenderOutputSize, context.ChunkResolution,
                    context.ProcessingColorSpace, 22);

                DrawLayerOnTexture(context, tempSurface.DrawingSurface, context.ChunkResolution, useFilters, targetPaint);

                blendPaint.SetFilters(null);
                DrawWithResolution(tempSurface.DrawingSurface, renderOnto, context.ChunkResolution);
            }

            return;
        }

        VecI size = AllowHighDpiRendering
            ? renderOnto.DeviceClipBounds.Size + renderOnto.DeviceClipBounds.Pos
            : context.RenderOutputSize;
        int saved = renderOnto.Canvas.Save();

        var adjustedResolution = AllowHighDpiRendering ? ChunkResolution.Full : context.ChunkResolution;

        var outputWorkingSurface =
            TryInitWorkingSurface(size, adjustedResolution, context.ProcessingColorSpace, 1);
        outputWorkingSurface.DrawingSurface.Canvas.Clear();
        outputWorkingSurface.DrawingSurface.Canvas.Save();
        if (AllowHighDpiRendering)
        {
            outputWorkingSurface.DrawingSurface.Canvas.SetMatrix(renderOnto.Canvas.TotalMatrix);
            renderOnto.Canvas.SetMatrix(Matrix3X3.Identity);
        }

        using var paint = new Paint
        {
            Color = new Color(255, 255, 255, 255), BlendMode = Drawie.Backend.Core.Surfaces.BlendMode.SrcOver
        };

        DrawLayerOnTexture(context, outputWorkingSurface.DrawingSurface, adjustedResolution, false, paint);

        ApplyMaskIfPresent(outputWorkingSurface.DrawingSurface, context, adjustedResolution);

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
                DrawClipSource(tempSurface.DrawingSurface, clipSource, context);
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

        DrawWithResolution(outputWorkingSurface.DrawingSurface, renderOnto, adjustedResolution);

        renderOnto.Canvas.RestoreToCount(saved);
        outputWorkingSurface.DrawingSurface.Canvas.Restore();
    }

    protected internal virtual void DrawLayerOnTexture(SceneObjectRenderContext ctx,
        DrawingSurface workingSurface,
        ChunkResolution resolution,
        bool useFilters, Paint paint)
    {
        int scaled = workingSurface.Canvas.Save();
        workingSurface.Canvas.Scale((float)resolution.Multiplier());

        DrawLayerOnto(ctx, workingSurface, useFilters, paint);

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


    protected internal virtual void DrawLayerInScene(SceneObjectRenderContext ctx,
        DrawingSurface workingSurface,
        bool useFilters = true)
    {
        DrawLayerOnto(ctx, workingSurface, useFilters, blendPaint);
    }

    protected void DrawLayerOnto(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        bool useFilters, Paint paint)
    {
        paint.Color = paint.Color.WithAlpha((byte)Math.Round(Opacity.Value * ctx.Opacity * 255));

        var targetSurface = workingSurface;
        Texture? tex = null;
        int saved = -1;
        if (!ctx.ProcessingColorSpace.IsSrgb && useFilters && Filters.Value != null)
        {
            saved = workingSurface.Canvas.Save();

            tex = Texture.ForProcessing(workingSurface,
                ColorSpace.CreateSrgb());
            workingSurface.Canvas.SetMatrix(Matrix3X3.Identity);

            targetSurface = tex.DrawingSurface;
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
            workingSurface.Canvas.DrawSurface(targetSurface, 0, 0);
            tex.Dispose();
            workingSurface.Canvas.RestoreToCount(saved);
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

    void IClipSource.DrawClipSource(SceneObjectRenderContext context, DrawingSurface drawOnto)
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
