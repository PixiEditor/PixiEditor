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
            Output.Value = Background.Value; 
            return;
        }

        blendPaint.Color = new Color(255, 255, 255, 255);
        blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.SrcOver;

        VecI targetSize = GetTargetSize(sceneContext);

        RenderContent(targetSize, sceneContext, sceneContext.RenderSurface,
            sceneContext.TargetPropertyOutput != FilterlessOutput);
    }

    private void RenderContent(VecI size, SceneObjectRenderContext context, DrawingSurface renderOnto, bool useFilters)
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

        var outputWorkingSurface = TryInitWorkingSurface(size, context.ChunkResolution, 1);
        outputWorkingSurface.DrawingSurface.Canvas.Clear();

        DrawLayerOnTexture(context, outputWorkingSurface.DrawingSurface, useFilters);

        ApplyMaskIfPresent(outputWorkingSurface.DrawingSurface, context);

        if (Background.Value != null)
        {
            Texture tempSurface = TryInitWorkingSurface(size, context.ChunkResolution, 4);
            tempSurface.DrawingSurface.Canvas.Clear();
            if (Background.Connection.Node is IClipSource clipSource && ClipToPreviousMember)
            {
                DrawClipSource(tempSurface.DrawingSurface, clipSource, context);
            }

            ApplyRasterClip(outputWorkingSurface.DrawingSurface, tempSurface.DrawingSurface);
        }

        DrawWithResolution(outputWorkingSurface.DrawingSurface, renderOnto, context.ChunkResolution, size);
    }

    protected internal virtual void DrawLayerOnTexture(SceneObjectRenderContext ctx, DrawingSurface workingSurface,
        bool useFilters)
    {
        int scaled = workingSurface.Canvas.Save();
        workingSurface.Canvas.Translate(GetScenePosition(ctx.FrameTime));

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
