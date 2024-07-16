using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surface.PaintImpl;

public class ColorFilter : NativeObject
{
    public override object Native => DrawingBackendApi.Current.ColorFilterImplementation.GetNativeColorFilter(ObjectPointer);
    public ColorFilter(IntPtr objPtr) : base(objPtr)
    {
        
    }

    public static ColorFilter CreateBlendMode(Color color, BlendMode blendMode)
    {
        ColorFilter filter = new ColorFilter(DrawingBackendApi.Current.ColorFilterImplementation.CreateBlendMode(color, blendMode));
        return filter;
    }

    /// <param name="matrix">An array of <see cref="F:SkiaSharp.SKColorFilter.ColorMatrixSize" /> elements.</param>
    /// <summary>Creates a new color filter that transforms a color by a 4x5 (row-major) matrix.</summary>
    /// <returns>Returns the new <see cref="T:PixiEditor.DrawingApi.Core.Surface.PaintImpl.ColorFilter" />.</returns>
    /// <remarks>The matrix is in row-major order and the translation column is specified in unnormalized, 0...255, space.</remarks>
    public static ColorFilter CreateColorMatrix(float[] matrix)
    {
        return new ColorFilter(DrawingBackendApi.Current.ColorFilterImplementation.CreateColorMatrix(matrix));
    }

    public static ColorFilter CreateColorMatrix(Matrix4x5F matrix)
    {
        float[] values = new float[Matrix4x5F.Width * Matrix4x5F.Height];
        matrix.TryGetMembers(values);

        return CreateColorMatrix(values);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.ColorFilterImplementation.Dispose(this);
    }
}
