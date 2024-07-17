using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;

public class CombineChannelsNode : Node
{
    private readonly Paint _screenPaint = new() { BlendMode = BlendMode.Screen };
    private readonly Paint _clearPaint = new() { BlendMode = BlendMode.DstIn };
    
    private readonly ColorFilter _redFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseRed + ColorMatrix.OpaqueAlphaOffset);
    private readonly ColorFilter _greenFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseGreen + ColorMatrix.OpaqueAlphaOffset);
    private readonly ColorFilter _blueFilter = ColorFilter.CreateColorMatrix(ColorMatrix.UseBlue + ColorMatrix.OpaqueAlphaOffset);
    
    private readonly ColorFilter _alphaGrayscaleFilter = ColorFilter.CreateColorMatrix(new ColorMatrix(
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (0, 0, 0, 0, 0),
        (1, 0, 0, 0, 0)));

    public InputProperty<Surface> Red { get; }
    
    public InputProperty<Surface> Green { get; }
    
    public InputProperty<Surface> Blue { get; }
    
    public InputProperty<Surface> Alpha { get; }

    public OutputProperty<Surface> Image { get; }
    
    // TODO: Either use a shader to combine each, or find a way to automatically "detect" if alpha channel is grayscale or not, oooor find an even better solution
    public InputProperty<bool> Grayscale { get; }

    public CombineChannelsNode()
    {
        Red = CreateInput<Surface>(nameof(Red), "RED", null);
        Green = CreateInput<Surface>(nameof(Green), "GREEN", null);
        Blue = CreateInput<Surface>(nameof(Blue), "BLUE", null);
        Alpha = CreateInput<Surface>(nameof(Alpha), "ALPHA", null);
        
        Image = CreateOutput<Surface>(nameof(Image), "IMAGE", null);
        Grayscale = CreateInput(nameof(Grayscale), "GRAYSCALE", false);
    }
    
    protected override Surface? OnExecute(RenderingContext context)
    {
        var size = GetSize();

        if (size == VecI.Zero)
            return null;
        
        var workingSurface = new Surface(size);

        if (Red.Value is { } red)
        {
            _screenPaint.ColorFilter = _redFilter;
            workingSurface.DrawingSurface.Canvas.DrawSurface(red.DrawingSurface, 0, 0, _screenPaint);
        }

        if (Green.Value is { } green)
        {
            _screenPaint.ColorFilter = _greenFilter;
            workingSurface.DrawingSurface.Canvas.DrawSurface(green.DrawingSurface, 0, 0, _screenPaint);
        }

        if (Blue.Value is { } blue)
        {
            _screenPaint.ColorFilter = _blueFilter;
            workingSurface.DrawingSurface.Canvas.DrawSurface(blue.DrawingSurface, 0, 0, _screenPaint);
        }

        if (Alpha.Value is { } alpha)
        {
            _clearPaint.ColorFilter = Grayscale.Value ? _alphaGrayscaleFilter : null;

            workingSurface.DrawingSurface.Canvas.DrawSurface(alpha.DrawingSurface, 0, 0, _clearPaint);
        }

        Image.Value = workingSurface;

        return workingSurface;
    }

    private VecI GetSize()
    {
        var final = new RectI();

        if (Red.Value is { } red)
        {
            final = final.Union(new RectI(VecI.Zero, red.Size));
        }

        if (Green.Value is { } green)
        {
            final = final.Union(new RectI(VecI.Zero, green.Size));
        }

        if (Blue.Value is { } blue)
        {
            final = final.Union(new RectI(VecI.Zero, blue.Size));
        }

        if (Alpha.Value is { } alpha)
        {
            final = final.Union(new RectI(VecI.Zero, alpha.Size));
        }

        return final.Size;
    }

    public override bool Validate() => true;

    public override Node CreateCopy() => new CombineChannelsNode();
}
