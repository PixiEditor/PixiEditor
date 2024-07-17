using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surface.PaintImpl;

public class ImageFilter : NativeObject
{
    public ImageFilter(IntPtr objPtr) : base(objPtr)
    {
    }

    public static ImageFilter CreateMatrixConvolution(VecI size, ReadOnlySpan<float> kernel, float gain, float bias, VecI kernelOffset,
        TileMode tileMode, bool convolveAlpha)
    {
        var filter = new ImageFilter(DrawingBackendApi.Current.ImageFilterImplementation.CreateMatrixConvolution(
            size,
            kernel,
            gain,
            bias,
            kernelOffset,
            tileMode,
            convolveAlpha));

        return filter;
    }

    public static ImageFilter CreateMatrixConvolution(Kernel kernel, float gain, float bias, VecI kernelOffset, TileMode tileMode, bool convolveAlpha) =>
        CreateMatrixConvolution(new VecI(kernel.Width, kernel.Height), kernel.AsSpan(), gain, bias, kernelOffset, tileMode, convolveAlpha);

    public static ImageFilter CreateMatrixConvolution(KernelArray kernel, float gain, float bias, VecI kernelOffset, TileMode tileMode, bool convolveAlpha) =>
        CreateMatrixConvolution(new VecI(kernel.Width, kernel.Height), kernel.AsSpan(), gain, bias, kernelOffset, tileMode, convolveAlpha);

    public override object Native => DrawingBackendApi.Current.ImageFilterImplementation.GetNativeImageFilter(ObjectPointer);
    
    public override void Dispose()
    {
        DrawingBackendApi.Current.ImageFilterImplementation.DisposeObject(ObjectPointer);
    }
}
