using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public abstract class LayerNode : StructureNode, IReadOnlyLayerNode
{
    protected Dictionary<(ChunkResolution, int), Texture> workingSurfaces =
        new Dictionary<(ChunkResolution, int), Texture>();

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);
    }

    public override void Render(SceneObjectRenderContext sceneContext)
    {
        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = Background.Value;
            return;
        }

        blendPaint.Color = new Color(255, 255, 255, 255);
        blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src;

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

        var rendered = RenderImage(targetSize, sceneContext, shouldClear);
        //TODO: optimize
        sceneContext.TargetSurface.Canvas.DrawSurface(rendered.DrawingSurface, 0, 0);
        
        Output.Value = sceneContext.TargetSurface;

    }

    private Texture RenderImage(VecI size, RenderContext context, bool shouldClear)
    {
        if (Output.Connections.Count > 0)
        {
            var outputWorkingSurface = TryInitWorkingSurface(size, context.ChunkResolution, 1);

            if (!HasOperations())
            {
                if (Background.Value != null)
                {
                    DrawBackground(outputWorkingSurface.DrawingSurface, context);
                    blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
                }

                DrawLayer(context, outputWorkingSurface, shouldClear);

                return outputWorkingSurface;
            }

            DrawLayer(context, outputWorkingSurface, true);

            // shit gets downhill with mask on big canvases, TODO: optimize
            ApplyMaskIfPresent(outputWorkingSurface, context);

            if (Background.Value != null)
            {
                Texture tempSurface = RequestTexture(4, outputWorkingSurface.Size, true);
                DrawBackground(tempSurface.DrawingSurface, context);
                ApplyRasterClip(outputWorkingSurface, tempSurface);
                blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
                tempSurface.DrawingSurface.Canvas.DrawSurface(outputWorkingSurface.DrawingSurface, 0, 0,
                    blendPaint);

                return tempSurface;
            }

            return outputWorkingSurface;
        }

        return null;
    }

    protected abstract VecI GetTargetSize(RenderContext ctx);

    protected virtual void DrawLayer(RenderContext ctx, Texture workingSurface, bool shouldClear,
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
    
    protected abstract void DrawWithoutFilters(RenderContext ctx, Texture workingSurface, bool shouldClear,
        Paint paint);
    
    protected abstract void DrawWithFilters(RenderContext ctx, Texture workingSurface, bool shouldClear,
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
