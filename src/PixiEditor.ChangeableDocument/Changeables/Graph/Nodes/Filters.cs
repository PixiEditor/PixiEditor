using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public static class Filters
{
    /// <summary>
    /// Maps red to the red, green and blue channels. Sets alpha to 1
    /// </summary>
    public static readonly ColorFilter RedGrayscaleFilter =
        ColorFilter.CreateColorMatrix(
            ColorMatrix.UseRed + ColorMatrix.MapRedToGreenBlue + ColorMatrix.OpaqueAlphaOffset);

    /// <summary>
    /// Maps green to the red, green and blue channels. Sets alpha to 1
    /// </summary>
    public static readonly ColorFilter GreenGrayscaleFilter =
        ColorFilter.CreateColorMatrix(ColorMatrix.UseGreen + ColorMatrix.MapGreenToRedBlue +
                                      ColorMatrix.OpaqueAlphaOffset);

    /// <summary>
    /// Maps blue to the red, green and blue channels. Sets alpha to 1
    /// </summary>
    public static readonly ColorFilter BlueGrayscaleFilter =
        ColorFilter.CreateColorMatrix(ColorMatrix.UseBlue + ColorMatrix.MapBlueToRedGreen +
                                      ColorMatrix.OpaqueAlphaOffset);

    /// <summary>
    /// Maps alpha to the red, green and blue channels. Sets alpha to 1
    /// </summary>
    public static readonly ColorFilter AlphaGrayscaleFilter =
        ColorFilter.CreateColorMatrix(ColorMatrix.MapAlphaToRedGreenBlue + ColorMatrix.OpaqueAlphaOffset);
    
    /// <summary>
    /// The rgb values become averaged into a grayscale image. Sets alpha to 1 <br/>
    /// </summary>
    public static readonly ColorFilter AverageGrayscaleFilter =
        ColorFilter.CreateColorMatrix(ColorMatrix.AverageGrayscale + ColorMatrix.OpaqueAlphaOffset);

    /// <summary>
    ///     R,G,B values are set to 0. Alpha is set to the average of R,G,B values. Multiplied by alpha
    /// </summary>
    public static readonly ColorFilter MaskFilter = ColorFilter.CreateLumaColor();

}
