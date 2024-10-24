using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Helpers;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

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
        return bounds;
    }

    public bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame, string elementToRenderName)
    {
        if (Image.Value == null)
            return false;

        RectD? bounds = GetPreviewBounds(frame, elementToRenderName);
        
        if (bounds == null)
            return false;
        
        RenderContext context = new(renderOn, frame, resolution, VecI.One);
        
        _paint.ColorFilter = Grayscale.Value ? _redGrayscaleFilter : _redFilter;
        RectD localBounds = new(bounds.Value.X, bounds.Value.Y, bounds.Value.Width / 2, bounds.Value.Height / 2);
        int saved = renderOn.Canvas.SaveLayer(_paint, localBounds);

        renderOn.Canvas.Scale(0.5f);
        renderOn.Canvas.Translate((float)bounds.Value.X, (float)bounds.Value.Y);
        
        Image.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);
        
        _paint.ColorFilter = Grayscale.Value ? _greenGrayscaleFilter : _greenFilter;
        localBounds = new(bounds.Value.X + bounds.Value.Width / 2, bounds.Value.Y, bounds.Value.Width / 2, bounds.Value.Height / 2);
        saved = renderOn.Canvas.SaveLayer(_paint, localBounds);
        
        renderOn.Canvas.Scale(0.5f);
        renderOn.Canvas.Translate((float)bounds.Value.X + (float)bounds.Value.Width, (float)bounds.Value.Y);
        
        Image.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);
        
        _paint.ColorFilter = Grayscale.Value ? _blueGrayscaleFilter : _blueFilter;
        localBounds = new(bounds.Value.X, bounds.Value.Y + bounds.Value.Height / 2, bounds.Value.Width / 2, bounds.Value.Height / 2);
        
        saved = renderOn.Canvas.SaveLayer(_paint, localBounds);
        
        renderOn.Canvas.Scale(0.5f);
        renderOn.Canvas.Translate((float)bounds.Value.X, (float)bounds.Value.Y + (float)bounds.Value.Height);
        
        Image.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);
        
        _paint.ColorFilter = Grayscale.Value ? _alphaGrayscaleFilter : _alphaFilter;
        localBounds = new(bounds.Value.X + bounds.Value.Width / 2, bounds.Value.Y + bounds.Value.Height / 2, bounds.Value.Width / 2, bounds.Value.Height / 2);
        saved = renderOn.Canvas.SaveLayer(_paint, localBounds);
        
        renderOn.Canvas.Scale(0.5f);
        
        renderOn.Canvas.Translate((float)bounds.Value.X + (float)bounds.Value.Width, (float)bounds.Value.Y + (float)bounds.Value.Height);
        
        Image.Value.Paint(context, renderOn);
        
        renderOn.Canvas.RestoreToCount(saved);

        return true;
    }
}
