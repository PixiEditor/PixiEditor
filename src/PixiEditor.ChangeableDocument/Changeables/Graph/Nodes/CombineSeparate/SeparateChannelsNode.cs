using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateChannels")]
public class SeparateChannelsNode : Node
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

    public OutputProperty<Surface?> Red { get; }
    
    public OutputProperty<Surface?> Green { get; }
    
    public OutputProperty<Surface?> Blue { get; }

    public OutputProperty<Surface?> Alpha { get; }
    
    public InputProperty<Surface?> Image { get; }
    
    public InputProperty<bool> Grayscale { get; }

    public SeparateChannelsNode()
    {
        Red = CreateOutput<Surface>(nameof(Red), "RED", null);
        Green = CreateOutput<Surface>(nameof(Green), "GREEN", null);
        Blue = CreateOutput<Surface>(nameof(Blue), "BLUE", null);
        Alpha = CreateOutput<Surface>(nameof(Alpha), "ALPHA", null);
        
        Image = CreateInput<Surface>(nameof(Image), "IMAGE", null);
        Grayscale = CreateInput(nameof(Grayscale), "GRAYSCALE", false);
    }


    public override string DisplayName { get; set; } = "SEPARATE_CHANNELS_NODE";
    
    protected override Surface? OnExecute(RenderingContext context)
    {
        var image = Image.Value;

        if (image == null)
            return null;
        
        var grayscale = Grayscale.Value;

        var red = !grayscale ? _redFilter : _redGrayscaleFilter;
        var green = !grayscale ? _greenFilter : _greenGrayscaleFilter;
        var blue = !grayscale ? _blueFilter : _blueGrayscaleFilter;
        var alpha = !grayscale ? _alphaFilter : _alphaGrayscaleFilter;

        Red.Value = GetImage(image, red);
        Green.Value = GetImage(image, green);
        Blue.Value = GetImage(image, blue);
        Alpha.Value = GetImage(image, alpha);

        var previewSurface = new Surface(image.Size * 2);

        var size = image.Size;
        
        var redPos = new VecI();
        var greenPos = new VecI(size.X, 0);
        var bluePos = new VecI(0, size.Y);
        var alphaPos = new VecI(size.X, size.Y);
        
        previewSurface.DrawingSurface.Canvas.DrawSurface(Red.Value.DrawingSurface, redPos, context.ReplacingPaintWithOpacity);
        previewSurface.DrawingSurface.Canvas.DrawSurface(Green.Value.DrawingSurface, greenPos, context.ReplacingPaintWithOpacity);
        previewSurface.DrawingSurface.Canvas.DrawSurface(Blue.Value.DrawingSurface, bluePos, context.ReplacingPaintWithOpacity);
        previewSurface.DrawingSurface.Canvas.DrawSurface(Alpha.Value.DrawingSurface, alphaPos, context.ReplacingPaintWithOpacity);
        
        return previewSurface;
    }

    private Surface GetImage(Surface image, ColorFilter filter)
    {
        var imageSurface = new Surface(image.Size);

        _paint.ColorFilter = filter;
        imageSurface.DrawingSurface.Canvas.DrawSurface(image.DrawingSurface, 0, 0, _paint);

        return imageSurface;
    }


    public override Node CreateCopy() => new SeparateChannelsNode();
}
