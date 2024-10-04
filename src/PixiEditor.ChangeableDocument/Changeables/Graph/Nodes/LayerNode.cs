using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class LayerNode : StructureNode, IReadOnlyLayerNode
{
    protected Dictionary<(ChunkResolution, int), Texture> workingSurfaces =
        new Dictionary<(ChunkResolution, int), Texture>();

    public override void Render(SceneObjectRenderContext sceneContext)
    {
        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = sceneContext.TargetSurface;
            return;
        }

        blendPaint.Color = new Color(255, 255, 255, 255);
        blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.SrcOver;

        DrawingSurface target = sceneContext.TargetSurface;

        VecI targetSize = GetTargetSize(sceneContext);
        bool shouldClear = Background.Value == null;

        /*if (FilterlessOutput.Connections.Count > 0)
        {
            var filterlessWorkingSurface = TryInitWorkingSurface(targetSize, context, 0);

            if (Background.Value != null)
            {
                DrawBackground(filterlessWorkingSurface.DrawingSurface, context);
                blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            }

            DrawLayer(context, filterlessWorkingSurface, shouldClear, useFilters: false);
            blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src;

            FilterlessOutput.Value = filterlessWorkingSurface;
        }*/

        RenderImage(targetSize, sceneContext, target, shouldClear);

        Output.Value = target;
    }

    private void RenderImage(VecI size, SceneObjectRenderContext context, DrawingSurface renderOnto, bool shouldClear)
    {
        if (Output.Connections.Count > 0)
        {
            if (!HasOperations())
            {
                /*
                if (Background.Value != null)
                {
                    DrawBackground(outputWorkingSurface.DrawingSurface, context);
                    blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
                }
                */

                DrawLayer(context, renderOnto, shouldClear);

                return;
            }

            //var outputWorkingSurface = TryInitWorkingSurface(size, context.ChunkResolution, 1);
            
            DrawLayer(context, renderOnto, true);

            // shit gets downhill with mask on big canvases, TODO: optimize
            ApplyMaskIfPresent(renderOnto, context);

            if (Background.Value != null)
            {
                Texture tempSurface = RequestTexture(4, size);
                DrawBackground(tempSurface.DrawingSurface, context);
                ApplyRasterClip(renderOnto, tempSurface.DrawingSurface);
                blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
                tempSurface.DrawingSurface.Canvas.DrawSurface(renderOnto, 0, 0,
                    blendPaint);

                renderOnto.Canvas.DrawSurface(tempSurface.DrawingSurface, VecI.Zero, blendPaint);
            }
        }
    }

    protected abstract VecI GetTargetSize(RenderContext ctx);

    protected virtual void DrawLayer(SceneObjectRenderContext ctx, DrawingSurface workingSurface, bool shouldClear,
        bool useFilters = true)
    {
        blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255));

        if (useFilters && Filters.Value != null)
        {
            blendPaint.SetFilters(Filters.Value);
            DrawWithFilters(ctx, workingSurface, shouldClear, blendPaint);
        }
        else
        {
            blendPaint.SetFilters(null);
            DrawWithoutFilters(ctx, workingSurface, shouldClear, blendPaint);
        }
    }

    protected abstract void DrawWithoutFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface, bool shouldClear,
        Paint paint);

    protected abstract void DrawWithFilters(SceneObjectRenderContext ctx, DrawingSurface workingSurface, bool shouldClear,
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

    public abstract bool RenderPreview(Texture renderOn, VecI chunk, ChunkResolution resolution, int frame);
}
