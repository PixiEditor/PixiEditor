using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

[NodeInfo("SeparateChannels", "SEPARATE_CHANNELS_NODE", Category = "IMAGE")]
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

    public OutputProperty<Texture?> Red { get; }
    
    public OutputProperty<Texture?> Green { get; }
    
    public OutputProperty<Texture?> Blue { get; }

    public OutputProperty<Texture?> Alpha { get; }
    
    public InputProperty<Texture?> Image { get; }
    
    public InputProperty<bool> Grayscale { get; }

    public SeparateChannelsNode()
    {
        Red = CreateOutput<Texture>(nameof(Red), "RED", null);
        Green = CreateOutput<Texture>(nameof(Green), "GREEN", null);
        Blue = CreateOutput<Texture>(nameof(Blue), "BLUE", null);
        Alpha = CreateOutput<Texture>(nameof(Alpha), "ALPHA", null);
        
        Image = CreateInput<Texture>(nameof(Image), "IMAGE", null);
        Grayscale = CreateInput(nameof(Grayscale), "GRAYSCALE", false);
    }
    
    protected override Texture? OnExecute(RenderingContext context)
    {
        var image = Image.Value;

        if (image == null)
            return null;
        
        var grayscale = Grayscale.Value;

        var red = !grayscale ? _redFilter : _redGrayscaleFilter;
        var green = !grayscale ? _greenFilter : _greenGrayscaleFilter;
        var blue = !grayscale ? _blueFilter : _blueGrayscaleFilter;
        var alpha = !grayscale ? _alphaFilter : _alphaGrayscaleFilter;

        Red.Value = GetImage(image, red, 0);
        Green.Value = GetImage(image, green, 1);
        Blue.Value = GetImage(image, blue, 2);
        Alpha.Value = GetImage(image, alpha, 3);

        var previewSurface = RequestTexture(4, image.Size * 2);

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

    private Texture GetImage(Texture image, ColorFilter filter, int id)
    {
        var imageSurface = RequestTexture(id, image.Size);

        _paint.ColorFilter = filter;
        imageSurface.DrawingSurface.Canvas.DrawSurface(image.DrawingSurface, 0, 0, _paint);

        return imageSurface;
    }


    public override Node CreateCopy() => new SeparateChannelsNode();
}
