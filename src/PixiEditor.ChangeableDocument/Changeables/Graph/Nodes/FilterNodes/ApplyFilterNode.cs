using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("ApplyFilter")]
public sealed class ApplyFilterNode : RenderNode, IRenderInput
{
    private readonly Paint _paint = new();
    private readonly Paint _maskPaint = new()
    {
        BlendMode = BlendMode.DstIn,
        ColorFilter = Filters.MaskFilter
    };

    public InputProperty<Filter?> Filter { get; }

    public RenderInputProperty Background { get; }

    public RenderInputProperty Mask { get; }
    
    public InputProperty<bool> InvertMask { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;

    public ApplyFilterNode()
    {
        Background = CreateRenderInput("Input", "IMAGE");
        Filter = CreateInput<Filter>("Filter", "FILTER", null);
        Mask = CreateRenderInput("Mask", "MASK");
        InvertMask = CreateInput("InvertMask", "INVERT_MASK", false);
        Output.FirstInChain = null;
        AllowHighDpiRendering = true;
    }

    protected override void Paint(RenderContext context, Canvas surface)
    {
        AllowHighDpiRendering = (Background.Connection?.Node as RenderNode)?.AllowHighDpiRendering ?? true;
        base.Paint(context, surface);
    }

    protected override void OnPaint(RenderContext context, Canvas outputSurface)
    {
        using var _ = DetermineTargetSurface(context, outputSurface, out var processingSurface);

        DrawWithFilter(context, outputSurface, processingSurface);
        
        // If the Mask is null, we already drew to the output surface, otherwise we still need to draw to it (and apply the mask)
        if (processingSurface != outputSurface)
        {
            ApplyWithMask(context, processingSurface, outputSurface);
        }
    }

    private void DrawWithFilter(RenderContext context, Canvas outputSurface, Canvas processingSurface)
    {
        _paint.SetFilters(Filter.Value);

        Render(context, outputSurface, processingSurface);
    }

    private void Render(RenderContext context, Canvas surface, Canvas targetSurface)
    {
        using var intermediate = Texture.ForProcessing(surface, context.ProcessingColorSpace);
        Texture? srgbSurface = null;

        Background.Value?.Paint(context, intermediate.DrawingSurface.Canvas);

        var surfaceToUse = intermediate.DrawingSurface;
        if (!context.ProcessingColorSpace.IsSrgb)
        {
            srgbSurface = Texture.ForProcessing(intermediate.Size, ColorSpace.CreateSrgb());
            srgbSurface.DrawingSurface.Canvas.DrawSurface(intermediate.DrawingSurface.Canvas.Surface, 0, 0, _paint);
            surfaceToUse = srgbSurface.DrawingSurface;
        }

        var saved = targetSurface.Save();
        targetSurface.SetMatrix(Matrix3X3.Identity);

        targetSurface.DrawSurface(surfaceToUse, 0, 0, context.ProcessingColorSpace.IsSrgb ? _paint : null);
        targetSurface.RestoreToCount(saved);

        srgbSurface?.Dispose();
    }

    private Texture? DetermineTargetSurface(RenderContext context, Canvas outputSurface, out Canvas targetSurface)
    {
        targetSurface = outputSurface;
        
        if (Mask.Value == null)
            return null;
        
        Background.Value?.Paint(context, outputSurface);
        var texture = Texture.ForProcessing(outputSurface, context.ProcessingColorSpace);
        targetSurface = texture.DrawingSurface.Canvas;
        
        return texture;
    }

    private void ApplyWithMask(RenderContext context, Canvas processedSurface, Canvas finalSurface)
    {
        _maskPaint.BlendMode = !InvertMask.Value ? BlendMode.DstIn : BlendMode.DstOut;
        var maskLayer = processedSurface.SaveLayer(_maskPaint);
        Mask.Value?.Paint(context, processedSurface);
        processedSurface.RestoreToCount(maskLayer);

        var saved = finalSurface.Save();
        finalSurface.SetMatrix(Matrix3X3.Identity);

        finalSurface.DrawSurface(processedSurface.Surface, 0, 0);
        finalSurface.RestoreToCount(saved);
    }

    public override RectD? GetPreviewBounds(RenderContext ctx, string elementToRenderName = "") =>
        null;
        /*
        PreviewUtils.FindPreviewBounds(Background.Connection, ctx.FrameTime.Frame, elementToRenderName);
        */

    public override Node CreateCopy() => new ApplyFilterNode();

    public override void Dispose()
    {
        base.Dispose();
        
        _paint.Dispose();
        _maskPaint.Dispose();
    }
}
