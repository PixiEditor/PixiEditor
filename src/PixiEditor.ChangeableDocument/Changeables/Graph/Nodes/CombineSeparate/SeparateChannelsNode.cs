using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateChannels")]
public class SeparateChannelsNode : Node, IRenderInput, IPreviewRenderable
{
    private readonly Paint _paint = new();
    
    private readonly ColorFilter _redFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseRed + ColorMatrix.OpaqueAlphaOffset);
    private readonly ColorFilter _greenFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseGreen + ColorMatrix.OpaqueAlphaOffset);
    private readonly ColorFilter _blueFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseBlue + ColorMatrix.OpaqueAlphaOffset);
    private readonly ColorFilter _alphaFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseAlpha);
    
    private readonly ColorFilter _redGrayscaleFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseRed + ColorMatrix.MapRedToGreenBlue + ColorMatrix.OpaqueAlphaOffset);
    private readonly ColorFilter _greenGrayscaleFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseGreen + ColorMatrix.MapGreenToRedBlue + ColorMatrix.OpaqueAlphaOffset);
    private readonly ColorFilter _blueGrayscaleFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseBlue + ColorMatrix.MapBlueToRedGreen + ColorMatrix.OpaqueAlphaOffset);
    private readonly ColorFilter _alphaGrayscaleFilter = ColorFilter.CreateColorMatrix(ColorMatrix.MapAlphaToRedGreenBlue + ColorMatrix.OpaqueAlphaOffset);

    public RenderOutputProperty Red { get; }
    
    public RenderOutputProperty Green { get; }
    
    public RenderOutputProperty Blue { get; }

    public RenderOutputProperty Alpha { get; }
    
    public RenderInputProperty Image { get; } 
    
    public InputProperty<bool> Grayscale { get; }

    public SeparateChannelsNode()
    {
        Red = CreateRenderOutput("Red", "RED", () => new Painter(PaintRed));
        Green = CreateRenderOutput("Green","GREEN", () => new Painter(PaintGreen)); 
        Blue = CreateRenderOutput("Blue", "BLUE", () => new Painter(PaintBlue));
        Alpha = CreateRenderOutput("Alpha", "ALPHA", () => new Painter(PaintAlpha));
        
        Image = CreateRenderInput("Image", "IMAGE");
        Grayscale = CreateInput(nameof(Grayscale), "GRAYSCALE", false);
    }
    
    private void PaintRed(RenderContext context, DrawingSurface drawingSurface)
    {
        Paint(context, drawingSurface, _redFilter, _redGrayscaleFilter);
    }
    
    private void PaintGreen(RenderContext context, DrawingSurface drawingSurface)
    {
        Paint(context, drawingSurface, _greenFilter, _greenGrayscaleFilter);
    }
    
    private void PaintBlue(RenderContext context, DrawingSurface drawingSurface)
    {
        Paint(context, drawingSurface, _blueFilter, _blueGrayscaleFilter);
    }
    
    private void PaintAlpha(RenderContext context, DrawingSurface drawingSurface)
    {
        Paint(context, drawingSurface, _alphaFilter, _alphaGrayscaleFilter);
    }

    private void Paint(RenderContext context, DrawingSurface drawingSurface, ColorFilter colorFilter, ColorFilter grayscaleFilter)
    {
        bool grayscale = Grayscale.Value;
        
        ColorFilter filter = grayscale ? grayscaleFilter : colorFilter; 
        _paint.ColorFilter = filter;
        
        int saved = drawingSurface.Canvas.SaveLayer(_paint);
        
        Image.Value.Paint(context, drawingSurface);
        
        drawingSurface.Canvas.RestoreToCount(saved);
    }

    protected override void OnExecute(RenderContext context)
    {
        Red.ChainToPainterValue();
        Green.ChainToPainterValue();
        Blue.ChainToPainterValue();
        Alpha.ChainToPainterValue();
    }

    public override Node CreateCopy() => new SeparateChannelsNode();
    RenderInputProperty IRenderInput.Background => Image;
    public RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        RectD? bounds = PreviewUtils.FindPreviewBounds(Image.Connection, frame, elementToRenderName);
        return bounds.HasValue ? new RectD(0, 0, bounds.Value.Width * 2, bounds.Value.Height * 2) : null;
    }

    public bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame, string elementToRenderName)
    {
        if (Image.Value == null)
            return false;

        RectD? bounds = GetPreviewBounds(frame, elementToRenderName);
        
        if (bounds == null)
            return false;

        using RenderContext context = new(renderOn, frame, resolution, VecI.One);
        
        _paint.ColorFilter = Grayscale.Value ? _redGrayscaleFilter : _redFilter;
        RectD localBounds = bounds.Value with { Width = bounds.Value.Width / 2, Height = bounds.Value.Height / 2, };
        int saved = renderOn.Canvas.SaveLayer(_paint, localBounds);
        
        renderOn.Canvas.Scale(0.8f, 0.8f);
        renderOn.Canvas.Translate((float)-bounds.Value.Width / 2f, (float)-bounds.Value.Height / 2f);
        
        Image.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);
        
        localBounds = new RectD(bounds.Value.Width / 2f, 0, bounds.Value.Width / 2, bounds.Value.Height / 2); 
        _paint.ColorFilter = Grayscale.Value ? _greenGrayscaleFilter : _greenFilter;
        saved = renderOn.Canvas.SaveLayer(_paint, localBounds);
        
        renderOn.Canvas.Scale(0.8f, 0.8f);
        renderOn.Canvas.Translate((float)bounds.Value.Width / 8f, (float)-bounds.Value.Height / 2f);
        Image.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);
        
        _paint.ColorFilter = Grayscale.Value ? _blueGrayscaleFilter : _blueFilter;
        localBounds = new RectD(0, bounds.Value.Height / 2f, bounds.Value.Width / 2, bounds.Value.Height / 2);
        saved = renderOn.Canvas.SaveLayer(_paint, localBounds);
        
        renderOn.Canvas.Scale(0.8f, 0.8f);
        renderOn.Canvas.Translate((float)-bounds.Value.Width / 2f, (float)bounds.Value.Height / 8f);
        Image.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);
        
        _paint.ColorFilter = Grayscale.Value ? _alphaGrayscaleFilter : _alphaFilter;
        localBounds = new RectD(bounds.Value.Width / 2f, bounds.Value.Height / 2f, bounds.Value.Width / 2, bounds.Value.Height / 2);
        saved = renderOn.Canvas.SaveLayer(_paint, localBounds);
        
        renderOn.Canvas.Scale(0.8f, 0.8f);
        renderOn.Canvas.Translate((float)bounds.Value.Width / 8f, (float)bounds.Value.Height / 8f);
        Image.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);

        return true;
    }
}
