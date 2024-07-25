using System;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IImageFilterImplementation
{
    IntPtr CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias, VecI kernelOffset, TileMode mode, bool convolveAlpha);

    IntPtr CreateCompose(ImageFilter outer, ImageFilter inner);

    object GetNativeImageFilter(IntPtr objPtr);
    
    void DisposeObject(IntPtr objPtr);
}
