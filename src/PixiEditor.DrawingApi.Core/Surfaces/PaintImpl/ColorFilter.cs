using System;
using PixiEditor.DrawingApi.Core.Bridge;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.Numerics;

namespace PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;

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

    /// <param name="outer">The outer (second) filter to apply.</param>
    /// <param name="inner">The inner (first) filter to apply.</param>
    /// <summary>Creates a new composition color filter, whose effect is to first apply the inner filter and then apply the outer filter to the result of the inner.</summary>
    /// <returns>Returns the new <see cref="T:PixiEditor.DrawingApi.Core.Surface.PaintImpl.ColorFilter" />.</returns>
    public static ColorFilter CreateCompose(ColorFilter outer, ColorFilter inner)
    {
        var handle = DrawingBackendApi.Current.ColorFilterImplementation.CreateCompose(outer, inner);
        ColorFilter filter = new ColorFilter(handle);

        return filter;
    }

    /// <summary>Creates a new color filter that transforms a color by a 4x5 (row-major) matrix.</summary>
    /// <returns>Returns the new <see cref="T:PixiEditor.DrawingApi.Core.Surface.PaintImpl.ColorFilter" />.</returns>
    /// <remarks>The matrix is in row-major order and the translation column is specified in unnormalized, 0...255, space.</remarks>
    public static ColorFilter CreateColorMatrix(float[] matrix)
    {
        return new ColorFilter(DrawingBackendApi.Current.ColorFilterImplementation.CreateColorMatrix(matrix));
    }

    public static ColorFilter CreateColorMatrix(ColorMatrix matrix)
    {
        float[] values = new float[ColorMatrix.Width * ColorMatrix.Height];
        matrix.TryGetMembers(values);

        return CreateColorMatrix(values);
    }

    public override void Dispose()
    {
        DrawingBackendApi.Current.ColorFilterImplementation.Dispose(this);
    }
}
