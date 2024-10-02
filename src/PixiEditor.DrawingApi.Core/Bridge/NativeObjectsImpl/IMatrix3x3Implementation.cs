using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;

public interface IMatrix3X3Implementation
{
    public bool TryInvert(Matrix3X3 matrix, out Matrix3X3 inversedResult);
    public Matrix3X3 Concat(in Matrix3X3 first, in Matrix3X3 second);
    public Matrix3X3 PostConcat(in Matrix3X3 first, in Matrix3X3 second);
    public VecD MapPoint(Matrix3X3 matrix, int p0, int p1);
    public VecD MapPoint(Matrix3X3 matrix, VecD point);
}
