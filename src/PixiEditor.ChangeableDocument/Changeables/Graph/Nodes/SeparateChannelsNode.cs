using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

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

    public OutputProperty<Image?> Red { get; }
    
    public OutputProperty<Image?> Green { get; }
    
    public OutputProperty<Image?> Blue { get; }

    public OutputProperty<Image?> Alpha { get; }
    
    public InputProperty<Image?> Image { get; }
    
    public InputProperty<bool> Grayscale { get; }

    public SeparateChannelsNode()
    {
        Red = CreateOutput<Image>(nameof(Red), "RED", null);
        Green = CreateOutput<Image>(nameof(Green), "GREEN", null);
        Blue = CreateOutput<Image>(nameof(Blue), "BLUE", null);
        Alpha = CreateOutput<Image>(nameof(Alpha), "ALPHA", null);
        
        Image = CreateInput<Image>(nameof(Image), "IMAGE", null);
        Grayscale = CreateInput(nameof(Grayscale), "GRAYSCALE", false);
    }
    
    protected override Image OnExecute(RenderingContext context)
    {
        var image = Image.Value;
        var grayscale = Grayscale.Value;

        var red = !grayscale ? _redFilter : _redGrayscaleFilter;
        var green = !grayscale ? _greenFilter : _greenGrayscaleFilter;
        var blue = !grayscale ? _blueFilter : _blueGrayscaleFilter;
        var alpha = !grayscale ? _alphaFilter : _alphaGrayscaleFilter;

        Red.Value = GetImage(image, red);
        Green.Value = GetImage(image, green);
        Blue.Value = GetImage(image, blue);
        Alpha.Value = GetImage(image, alpha);

        using var previewSurface = new Surface(image.Size);

        var half = image.Size / 2;
        var halfX = half.X;
        var halfY = half.Y;
        
        var redRect = new RectD(new VecD(), half);
        var greenRect = new RectD(new VecD(halfX, 0), half);
        var blueRect = new RectD(new VecD(0, halfY), half);
        var alphaRect = new RectD(new VecD(halfX, halfY), half);
        
        previewSurface.DrawingSurface.Canvas.DrawImage(Red.Value, redRect, context.ReplacingPaintWithOpacity);
        previewSurface.DrawingSurface.Canvas.DrawImage(Green.Value, greenRect, context.ReplacingPaintWithOpacity);
        previewSurface.DrawingSurface.Canvas.DrawImage(Blue.Value, blueRect, context.ReplacingPaintWithOpacity);
        previewSurface.DrawingSurface.Canvas.DrawImage(Alpha.Value, alphaRect, context.ReplacingPaintWithOpacity);
        
        return previewSurface.DrawingSurface.Snapshot();
    }

    private Image GetImage(Image image, ColorFilter filter)
    {
        using var imageSurface = new Surface(image.Size);

        _paint.ColorFilter = filter;
        imageSurface.DrawingSurface.Canvas.DrawImage(image, 0, 0, _paint);

        return imageSurface.DrawingSurface.Snapshot();
    }

    public override bool Validate() => Image.Value != null;

    public override Node CreateCopy() => new SeparateChannelsNode();
}
