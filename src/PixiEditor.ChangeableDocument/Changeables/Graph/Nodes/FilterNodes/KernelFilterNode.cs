using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("KernelFilter")]
public class KernelFilterNode : FilterNode
{
    private readonly Paint _paint = new();
    
    public InputProperty<Kernel> Kernel { get; }
    
    public InputProperty<double> Gain { get; }

    public InputProperty<double> Bias { get; }

    public InputProperty<TileMode> Tile { get; }

    public InputProperty<bool> OnAlpha { get; }

    private ImageFilter filter;
    private Kernel lastKernel;
    private TileMode lastTile;
    private double lastGain;
    private double lastBias;
    private bool lastOnAlpha;

    private float[] lastKernelValues = new float[9];

    public KernelFilterNode()
    {
        Kernel = CreateInput(nameof(Kernel), "KERNEL", Drawie.Numerics.Kernel.Identity(3, 3));
        Gain = CreateInput(nameof(Gain), "GAIN", 1d);
        Bias = CreateInput(nameof(Bias), "BIAS", 0d);
        Tile = CreateInput(nameof(Tile), "TILE_MODE", TileMode.Clamp);
        OnAlpha = CreateInput(nameof(OnAlpha), "ON_ALPHA", false);
    }

    protected override ImageFilter? GetImageFilter()
    {
        var kernel = Kernel.Value;
        
        if (kernel.AsSpan().SequenceEqual(lastKernelValues) && Tile.Value == lastTile && Gain.Value == lastGain && Bias.Value == lastBias && OnAlpha.Value == lastOnAlpha)
            return filter;
        
        lastKernel = kernel;
        lastTile = Tile.Value;
        lastGain = Gain.Value;
        lastBias = Bias.Value;
        lastOnAlpha = OnAlpha.Value;
        
        filter?.Dispose();
        
        var kernelOffset = new VecI(kernel.RadiusX, kernel.RadiusY);
        
        filter = ImageFilter.CreateMatrixConvolution(kernel, (float)Gain.Value, (float)Bias.Value, kernelOffset, Tile.Value, OnAlpha.Value);
        lastKernelValues = kernel.AsSpan().ToArray();
        return filter;
    }

    public override Node CreateCopy() => new KernelFilterNode();
}
