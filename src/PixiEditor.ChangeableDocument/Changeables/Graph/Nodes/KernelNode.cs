using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class KernelFilterNode : Node
{
    private readonly Paint _paint = new();
    
    public OutputProperty<Surface> Transformed { get; }
    
    public InputProperty<Surface?> Image { get; }
    
    public InputProperty<Kernel> Kernel { get; }
    
    public InputProperty<double> Gain { get; }

    public InputProperty<double> Bias { get; }

    public InputProperty<TileMode> Tile { get; }

    public InputProperty<bool> OnAlpha { get; }

    public override string DisplayName { get; set; } = "KERNEL_FILTER_NODE";
    public KernelFilterNode()
    {
        Transformed = CreateOutput<Surface>(nameof(Transformed), "TRANSFORMED", null);
        Image = CreateInput<Surface>(nameof(Image), "IMAGE", null);
        Kernel = CreateInput(nameof(Kernel), "KERNEL", Numerics.Kernel.Identity(3, 3));
        Gain = CreateInput(nameof(Gain), "GAIN", 1d);
        Bias = CreateInput(nameof(Bias), "BIAS", 0d);
        Tile = CreateInput(nameof(Tile), "TILE_MODE", TileMode.Clamp);
        OnAlpha = CreateInput(nameof(OnAlpha), "ON_ALPHA", false);
    }

    protected override string NodeUniqueName => "KernelFilter";

    protected override Surface? OnExecute(RenderingContext context)
    {
        var input = Image.Value;

        if (input == null)
            return null;
        
        var kernel = Kernel.Value;
        var workingSurface = new Surface(input.Size);

        var kernelOffset = new VecI(kernel.RadiusX, kernel.RadiusY);
        using var imageFilter = ImageFilter.CreateMatrixConvolution(kernel, (float)Gain.Value, (float)Bias.Value, kernelOffset, Tile.Value, OnAlpha.Value);

        _paint.ImageFilter = imageFilter;
        workingSurface.DrawingSurface.Canvas.DrawSurface(Image.Value.DrawingSurface, 0, 0, _paint);
        
        Transformed.Value = workingSurface;
        
        return workingSurface;
    }


    public override Node CreateCopy() => new KernelFilterNode();
}
