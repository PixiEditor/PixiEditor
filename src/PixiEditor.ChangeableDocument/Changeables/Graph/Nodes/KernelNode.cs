using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.ImageData;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public class KernelFilterNode : FilterNode
{
    private readonly Paint _paint = new();
    
    public InputProperty<Kernel> Kernel { get; }
    
    public InputProperty<double> Gain { get; }

    public InputProperty<double> Bias { get; }

    public InputProperty<TileMode> Tile { get; }

    public InputProperty<bool> OnAlpha { get; }

    public override string DisplayName { get; set; } = "KERNEL_FILTER_NODE";
    public KernelFilterNode()
    {
        Kernel = CreateInput(nameof(Kernel), "KERNEL", Numerics.Kernel.Identity(3, 3));
        Gain = CreateInput(nameof(Gain), "GAIN", 1d);
        Bias = CreateInput(nameof(Bias), "BIAS", 0d);
        Tile = CreateInput(nameof(Tile), "TILE_MODE", TileMode.Clamp);
        OnAlpha = CreateInput(nameof(OnAlpha), "ON_ALPHA", false);
    }

    protected override string NodeUniqueName => "KernelFilter";

    protected override ImageFilter? GetImageFilter()
    {
        var kernel = Kernel.Value;
        
        var kernelOffset = new VecI(kernel.RadiusX, kernel.RadiusY);
        
        return ImageFilter.CreateMatrixConvolution(kernel, (float)Gain.Value, (float)Bias.Value, kernelOffset, Tile.Value, OnAlpha.Value);
    }

    public override Node CreateCopy() => new KernelFilterNode();
}
