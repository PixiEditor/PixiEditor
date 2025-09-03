using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Rendering;

public static class PreviewUtility
{
    public static ChunkResolution CalculateResolution(VecD size, VecD textureSize)
    {
        VecD densityVec = size.Divide(textureSize);
        double density = Math.Min(densityVec.X, densityVec.Y);
        return density switch
        {
            > 8.01 => ChunkResolution.Eighth,
            > 4.01 => ChunkResolution.Quarter,
            > 2.01 => ChunkResolution.Half,
            _ => ChunkResolution.Full
        };
    }

    public static VecD CalculateUniformScaling(VecD originalSize, VecD targetSize)
    {
        if (originalSize.X == 0 || originalSize.Y == 0)
            return new VecD(1);

        VecD scale = targetSize.Divide(originalSize);
        double uniformScale = Math.Min(scale.X, scale.Y);
        return new VecD(uniformScale, uniformScale);
    }

    public static VecD CalculateCenteringOffset(VecD originalSize, VecD targetSize, VecD scaling)
    {
        if (originalSize.X == 0 || originalSize.Y == 0)
            return VecD.Zero;

        VecD scaledOriginal = originalSize.Multiply(scaling);
        return (targetSize - scaledOriginal) / 2;
    }

    public static RenderContext CreatePreviewContext(RenderContext ctx, VecD scaling, VecD renderSize, VecD textureSize)
    {
        var clone = ctx.Clone();
        clone.ChunkResolution = CalculateResolution(renderSize, textureSize);
        clone.DesiredSamplingOptions = scaling.X > 1 ? SamplingOptions.Default : SamplingOptions.Bilinear;

        return clone;
    }
}
