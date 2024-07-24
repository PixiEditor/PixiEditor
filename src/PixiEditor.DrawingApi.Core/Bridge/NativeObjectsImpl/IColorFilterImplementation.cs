using System;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IColorFilterImplementation
{
    public IntPtr CreateBlendMode(Color color, BlendMode blendMode);
    public IntPtr CreateColorMatrix(float[] matrix);
    public void Dispose(ColorFilter colorFilter);
    public object GetNativeColorFilter(IntPtr objectPointer);
}
