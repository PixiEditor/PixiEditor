using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

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
            Output.Value = sceneContext.RenderSurface;
            return;
        }

        blendPaint.Color = new Color(255, 255, 255, 255);
        blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.SrcOver;

        DrawingSurface target = sceneContext.RenderSurface;

        VecI targetSize = GetTargetSize(sceneContext);

        /*if (FilterlessOutput.Connections.Count > 0)
        {
            var filterlessWorkingSurface = TryInitWorkingSurface(targetSize, sceneContext.ChunkResolution, 0);

            if (Background.Value != null)
            {
                DrawBackground(filterlessWorkingSurface.DrawingSurface, sceneContext);
                blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            }

            DrawLayer(sceneContext, filterlessWorkingSurface.DrawingSurface, shouldClear, useFilters: false);
            blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src;

            FilterlessOutput.Value = filterlessWorkingSurface;
        }*/

        RenderContent(targetSize, sceneContext, target);

        Output.Value = target;
    }

    private void RenderContent(VecI size, SceneObjectRenderContext context, DrawingSurface renderOnto)
    {
        if (Output.Connections.Count > 0)
        {
            if (!HasOperations())
            {
                if (RenderTarget.Value != null)
                {
                    blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
                }

                DrawLayerInScene(context, renderOnto, true);
                return;
            }

            var outputWorkingSurface = TryInitWorkingSurface(size, context.ChunkResolution, 1);
            outputWorkingSurface.DrawingSurface.Canvas.Clear();

            DrawLayerOnTexture(context, outputWorkingSurface.DrawingSurface, true);

            ApplyMaskIfPresent(outputWorkingSurface.DrawingSurface, context);

            if (RenderTarget.Value != null)
            {
                Texture tempSurface = TryInitWorkingSurface(size, context.ChunkResolution, 4);
                tempSurface.DrawingSurface.Canvas.Clear();
                if (RenderTarget.Connection.Node is IClipSource clipSource)
                {
                    // TODO: This probably should work with StructureMembers not Layers only
                    DrawClipSource(tempSurface.DrawingSurface, clipSource, context);
                }

                ApplyRasterClip(outputWorkingSurface.DrawingSurface, tempSurface.DrawingSurface);
                blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
                tempSurface.DrawingSurface.Canvas.DrawSurface(outputWorkingSurface.DrawingSurface, 0, 0, blendPaint);

                DrawWithResolution(tempSurface.DrawingSurface, renderOnto, context.ChunkResolution, size);
                return;
            }

            DrawWithResolution(outputWorkingSurface.DrawingSurface, renderOnto, context.ChunkResolution, size);
        }
    }

    protected internal virtual void DrawLayerOnTexture(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        bool useFilters)
    {
        int scaled = workingSurface.Canvas.Save();
        workingSurface.Canvas.Translate(ScenePosition);

        DrawLayerOnto(ctx, workingSurface, useFilters);

        workingSurface.Canvas.RestoreToCount(scaled);
    }

    private void DrawWithResolution(DrawingSurface source, DrawingSurface target, ChunkResolution resolution, VecI size)
    {
        int scaled = target.Canvas.Save();
        float multiplier = (float)resolution.InvertedMultiplier();
        target.Canvas.Scale(multiplier, multiplier);

        target.Canvas.DrawSurface(source, 0, 0, blendPaint);

        target.Canvas.RestoreToCount(scaled);
    }

    protected abstract VecI GetTargetSize(RenderContext ctx);

    protected internal virtual void DrawLayerInScene(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        bool useFilters = true)
    {
        DrawLayerOnto(ctx, workingSurface, useFilters);
    }

    private void DrawLayerOnto(SceneObjectRenderContext ctx, DrawingSurface workingSurface, bool useFilters)
    {
        blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * ctx.Opacity * 255));

        if (useFilters && Filters.Value != null)
        {
            blendPaint.SetFilters(Filters.Value);
            DrawWithFilters(ctx, workingSurface, blendPaint);
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

    protected Texture TryInitWorkingSurface(VecI imageSize, ChunkResolution resolution, int id)
    {
        ChunkResolution targetResolution = resolution;
        bool hasSurface = workingSurfaces.TryGetValue((targetResolution, id), out Texture workingSurface);
        VecI targetSize = (VecI)(imageSize * targetResolution.Multiplier());

        targetSize = new VecI(Math.Max(1, targetSize.X), Math.Max(1, targetSize.Y));

        if (!hasSurface || workingSurface.Size != targetSize || workingSurface.IsDisposed)
        {
            workingSurfaces[(targetResolution, id)] = new Texture(targetSize);
            workingSurface = workingSurfaces[(targetResolution, id)];
        }

        return workingSurface;
    }

    void IClipSource.DrawOnTexture(SceneObjectRenderContext context, DrawingSurface drawOnto)
    {
        DrawLayerOnTexture(context, drawOnto, false);
    }
}
