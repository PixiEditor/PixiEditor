using System;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IImageFilterImplementation
{
    IntPtr CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias, VecI kernelOffset, TileMode mode, bool convolveAlpha);

    IntPtr CreateCompose(ImageFilter outer, ImageFilter inner);

    object GetNativeImageFilter(IntPtr objPtr);
    
    void DisposeObject(IntPtr objPtr);
}
