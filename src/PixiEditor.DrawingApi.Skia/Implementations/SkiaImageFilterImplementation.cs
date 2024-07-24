using System;
using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;
using SkiaSharp;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaImageFilterImplementation : SkObjectImplementation<SKImageFilter>, IImageFilterImplementation
    {
        public IntPtr CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias, VecI kernelOffset, TileMode mode, bool convolveAlpha)
        {
            var skImageFilter = SKImageFilter.CreateMatrixConvolution(
                new SKSizeI(size.X, size.Y),
                kernel,
                gain,
                bias,
                new SKPointI(kernelOffset.X, kernelOffset.Y),
                (SKShaderTileMode)mode,
                convolveAlpha);

            ManagedInstances[skImageFilter.Handle] = skImageFilter;
            return skImageFilter.Handle;
        }

        public object GetNativeImageFilter(IntPtr objPtr) => ManagedInstances[objPtr];
    }
}
