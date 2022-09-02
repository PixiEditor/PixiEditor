using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IMatrix3X3Implementation
{
    public bool TryInvert(Matrix3X3 matrix, out Matrix3X3 inversedResult);
    public Matrix3X3 Concat(in Matrix3X3 first, in Matrix3X3 second);
}
