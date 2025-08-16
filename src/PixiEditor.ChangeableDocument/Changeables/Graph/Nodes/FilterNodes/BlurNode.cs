using System.Diagnostics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;

[NodeInfo("BlurFilter")]
public class BlurNode : FilterNode
{
    public InputProperty<bool> PreserveAlpha { get; }
    
    public InputProperty<VecD> Radius { get; }

    protected override bool ExecuteOnlyOnCacheChange => true;

    public BlurNode()
    {
        PreserveAlpha = CreateInput("PreserveAlpha", "PRESERVE_ALPHA", true);
        Radius = CreateInput("Radius", "RADIUS", new VecD(1, 1)).WithRules(x => x.Min(new VecD(0, 0)));
    }

    protected override ImageFilter? GetImageFilter(RenderContext context)
    {
        var sigma = (VecF)(Radius.Value * context.ChunkResolution.Multiplier());
        var preserveAlpha = PreserveAlpha.Value;

        var xFilter = GetGaussianFilter(sigma.X, true, preserveAlpha, null, out float[] xKernel);
        
        // Reuse xKernel if x == y
        var yKernel = Math.Abs(sigma.Y - sigma.X) < 0.0001f ? xKernel : null;
        var yFilter = GetGaussianFilter(sigma.Y, false, preserveAlpha, yKernel, out _);

        return (xFilter, yFilter) switch
        {
            (null, _) => yFilter,
            (_, null) => xFilter,
            (_, _) => ImageFilter.CreateCompose(yFilter, xFilter)
        };
    }

    private static ImageFilter? GetGaussianFilter(float sigma, bool isX, bool preserveAlpha, float[]? kernel, out float[] usedKernel)
    {
        usedKernel = null;
        if (sigma < 0.0001f) return null;
        
        kernel ??= GenerateGaussianKernel(sigma);
        usedKernel = kernel;
        
        var size = isX ? new VecI(kernel.Length, 1) : new VecI(1, kernel.Length);
        var offset = isX ? new VecI(kernel.Length / 2, 0) : new VecI(0, kernel.Length / 2);
        
        return ImageFilter.CreateMatrixConvolution(size, kernel, 1, 0, offset, TileMode.Repeat, !preserveAlpha);
    }
    
    public static float[] GenerateGaussianKernel(float sigma)
    {
        int radius = (int)Math.Ceiling(3 * sigma);
        radius = Math.Min(radius, 300);
        int kernelSize = 2 * radius + 1;

        float[] kernel = new float[kernelSize];
        float sum = 0f;
        float twoSigmaSquare = 2 * sigma * sigma;

        for (int i = 0; i < kernelSize; i++)
        {
            int x = i - radius;
            kernel[i] = (float)Math.Exp(-(x * x) / twoSigmaSquare);
            sum += kernel[i];
        }

        // Normalize the kernel to ensure the sum of elements is 1
        for (int i = 0; i < kernelSize; i++)
        {
            kernel[i] /= sum;
        }

        return kernel;
    }

    public override Node CreateCopy() => new BlurNode();
}
