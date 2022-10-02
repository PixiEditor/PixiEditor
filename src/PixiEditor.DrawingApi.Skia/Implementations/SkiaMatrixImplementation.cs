using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.DrawingApi.Skia.Implementations
{
    public class SkiaMatrixImplementation : IMatrix3X3Implementation
    {
        public bool TryInvert(Matrix3X3 matrix, out Matrix3X3 inversedResult)
        {
            bool inverted = matrix.ToSkMatrix().TryInvert(out var result);
            inversedResult = result.ToMatrix3X3();
            
            return inverted;
        }

        public Matrix3X3 Concat(in Matrix3X3 first, in Matrix3X3 second)
        {
            throw new System.NotImplementedException();
        }

        public Matrix3X3 PostConcat(in Matrix3X3 first, in Matrix3X3 second)
        {
            throw new System.NotImplementedException();
        }

        public VecD MapPoint(Matrix3X3 matrix, int p0, int p1)
        {
            throw new System.NotImplementedException();
        }
    }
}
