using System;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IColorFilterImplementation
{
    public IntPtr CreateBlendMode(Color color, BlendMode blendMode);
    public void Dispose(ColorFilter colorFilter);
}
