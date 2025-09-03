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

    public ApplyFilterNode()
    {
        Background = CreateRenderInput("Input", "IMAGE");
        Filter = CreateInput<Filter>("Filter", "FILTER", null);
        Mask = CreateRenderInput("Mask", "MASK");
        InvertMask = CreateInput("InvertMask", "INVERT_MASK", false);
        Output.FirstInChain = null;
        AllowHighDpiRendering = true;
    }

    protected override void Paint(RenderContext context, DrawingSurface surface)
    {
        AllowHighDpiRendering = (Background.Connection.Node as RenderNode)?.AllowHighDpiRendering ?? true;
        base.Paint(context, surface);
    }

    protected override void OnPaint(RenderContext context, DrawingSurface outputSurface)
    {
        using var _ = DetermineTargetSurface(context, outputSurface, out var processingSurface);

        DrawWithFilter(context, outputSurface, processingSurface);
        
        // If the Mask is null, we already drew to the output surface, otherwise we still need to draw to it (and apply the mask)
        if (processingSurface != outputSurface)
        {
            ApplyWithMask(context, processingSurface, outputSurface);
        }
    }

    private void DrawWithFilter(RenderContext context, DrawingSurface outputSurface, DrawingSurface processingSurface)
    {
        _paint.SetFilters(Filter.Value);

        if (!context.ProcessingColorSpace.IsSrgb)
        {
            HandleNonSrgbContext(context, outputSurface, processingSurface);
            return;
        }

        var layer = processingSurface.Canvas.SaveLayer(_paint);
        Background.Value?.Paint(context, processingSurface);
        processingSurface.Canvas.RestoreToCount(layer);
    }

    private void HandleNonSrgbContext(RenderContext context, DrawingSurface surface, DrawingSurface targetSurface)
    {
        using var intermediate = Texture.ForProcessing(surface, context.ProcessingColorSpace);

        Background.Value?.Paint(context, intermediate.DrawingSurface);

        using var srgbSurface = Texture.ForProcessing(intermediate.Size, ColorSpace.CreateSrgb());

        srgbSurface.DrawingSurface.Canvas.SaveLayer(_paint);
        srgbSurface.DrawingSurface.Canvas.DrawSurface(intermediate.DrawingSurface, 0, 0);
        srgbSurface.DrawingSurface.Canvas.Restore();

        var saved = targetSurface.Canvas.Save();
        targetSurface.Canvas.SetMatrix(Matrix3X3.Identity);

        targetSurface.Canvas.DrawSurface(srgbSurface.DrawingSurface, 0, 0);
        targetSurface.Canvas.RestoreToCount(saved);
    }

    private Texture? DetermineTargetSurface(RenderContext context, DrawingSurface outputSurface, out DrawingSurface targetSurface)
    {
        targetSurface = outputSurface;
        
        if (Mask.Value == null)
            return null;
        
        Background.Value?.Paint(context, outputSurface);
        var texture = Texture.ForProcessing(outputSurface, context.ProcessingColorSpace);
        targetSurface = texture.DrawingSurface;
        
        return texture;
    }

    private void ApplyWithMask(RenderContext context, DrawingSurface processedSurface, DrawingSurface finalSurface)
    {
        _maskPaint.BlendMode = !InvertMask.Value ? BlendMode.DstIn : BlendMode.DstOut;
        var maskLayer = processedSurface.Canvas.SaveLayer(_maskPaint);
        Mask.Value?.Paint(context, processedSurface);
        processedSurface.Canvas.RestoreToCount(maskLayer);

        var saved = finalSurface.Canvas.Save();
        finalSurface.Canvas.SetMatrix(Matrix3X3.Identity);

        finalSurface.Canvas.DrawSurface(processedSurface, 0, 0);
        finalSurface.Canvas.RestoreToCount(saved);
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
