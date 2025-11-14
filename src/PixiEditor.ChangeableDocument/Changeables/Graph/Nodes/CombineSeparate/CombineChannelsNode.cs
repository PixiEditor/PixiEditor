using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("CombineChannels")]
public class CombineChannelsNode : RenderNode
{
    private readonly Paint _screenPaint = new() { BlendMode = BlendMode.Screen };
    private readonly Paint _clearPaint = new() { BlendMode = BlendMode.DstIn };
    
    private readonly ColorFilter _redFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseRed + ColorMatrix.OpaqueAlphaOffset);
    private readonly ColorFilter _greenFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseGreen + ColorMatrix.OpaqueAlphaOffset);
    private readonly ColorFilter _blueFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseBlue + ColorMatrix.OpaqueAlphaOffset);

    public RenderInputProperty Red { get; }
    
    public RenderInputProperty Green { get; }
    
    public RenderInputProperty Blue { get; }
    
    public RenderInputProperty Alpha { get; }

    // TODO: Either use a shader to combine each, or find a way to automatically "detect" if alpha channel is grayscale or not, oooor find an even better solution
    public InputProperty<bool> Grayscale { get; }

    public CombineChannelsNode()
    {
        Red = CreateRenderInput("Red", "RED");
        Green = CreateRenderInput("Green","GREEN");
        Blue = CreateRenderInput("Blue", "BLUE");
        Alpha = CreateRenderInput("Alpha", "ALPHA");
        
        Grayscale = CreateInput(nameof(Grayscale), "GRAYSCALE", false);
    }

    
    protected override void OnPaint(RenderContext context, Canvas surface)
    {
        int saved = surface.SaveLayer();
        if (Red.Value is { } red)
        {
            _screenPaint.ColorFilter = _redFilter;
            
            int savedRed = surface.SaveLayer(_screenPaint);
            red.Paint(context, surface);
            
            surface.RestoreToCount(savedRed);
        }

        if (Green.Value is { } green)
        {
            _screenPaint.ColorFilter = _greenFilter;
            int savedGreen = surface.SaveLayer(_screenPaint);
            green.Paint(context, surface);
            
            surface.RestoreToCount(savedGreen);
        }

        if (Blue.Value is { } blue)
        {
            _screenPaint.ColorFilter = _blueFilter;
            int savedBlue = surface.SaveLayer(_screenPaint);
            blue.Paint(context, surface);
            
            surface.RestoreToCount(savedBlue);
        }

        if (Alpha.Value is { } alpha)
        {
            _clearPaint.ColorFilter = Grayscale.Value ? Filters.AlphaGrayscaleFilter : null;
            int savedAlpha = surface.SaveLayer(_clearPaint);
            alpha.Paint(context, surface);
            
            surface.RestoreToCount(savedAlpha);
        }
            
        surface.RestoreToCount(saved);
    }

    public override RectD? GetPreviewBounds(RenderContext ctx, string elementToRenderName = "")
    {
        int frame = ctx.FrameTime.Frame;
        /*RectD? redBounds = PreviewUtils.FindPreviewBounds(Red.Connection, frame, elementToRenderName);
        RectD? greenBounds = PreviewUtils.FindPreviewBounds(Green.Connection, frame, elementToRenderName);
        RectD? blueBounds = PreviewUtils.FindPreviewBounds(Blue.Connection, frame, elementToRenderName);
        RectD? alphaBounds = PreviewUtils.FindPreviewBounds(Alpha.Connection, frame, elementToRenderName);

        RectD? finalBounds = null;
        
        if (redBounds == null && greenBounds == null && blueBounds == null && alphaBounds == null)
        {
            return null;
        }

        if (redBounds.HasValue)
        {
            finalBounds = redBounds.Value;
        }
        
        if (greenBounds.HasValue)
        {
            finalBounds = finalBounds?.Union(greenBounds.Value) ?? greenBounds.Value;
        }
        
        if (blueBounds.HasValue)
        {
            finalBounds = finalBounds?.Union(blueBounds.Value) ?? blueBounds.Value;
        }
        
        if (alphaBounds.HasValue)
        {
            finalBounds = finalBounds?.Union(alphaBounds.Value) ?? alphaBounds.Value;
        }
        
        return finalBounds;*/
        return null;
    }

    protected override bool ShouldRenderPreview(string elementToRenderName)
    {
        return Red.Value != null || Green.Value != null || Blue.Value != null || Alpha.Value != null;
    }

    public override void RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        OnPaint(context, renderOn.Canvas);
    }

    public override Node CreateCopy() => new CombineChannelsNode();
}
