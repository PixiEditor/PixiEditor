using PixiEditor.DrawingApi.Core.Bridge.NativeObjectsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

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
            return first.ToSkMatrix().PreConcat(second.ToSkMatrix()).ToMatrix3X3();
        }

        public Matrix3X3 PostConcat(in Matrix3X3 first, in Matrix3X3 second)
        {
            return first.ToSkMatrix().PostConcat(second.ToSkMatrix()).ToMatrix3X3();
        }

        public VecD MapPoint(Matrix3X3 matrix, int p0, int p1)
        {
            var mapped = matrix.ToSkMatrix().MapPoint(p0, p1);
            return new VecD(mapped.X, mapped.Y);
        }

        public VecD MapPoint(Matrix3X3 matrix, VecD point)
        {
            var mapped = matrix.ToSkMatrix().MapPoint((float)point.X, (float)point.Y);
            return new VecD(mapped.X, mapped.Y);
        }
    }
}
