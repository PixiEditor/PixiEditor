using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

public static class Filters
{
    public static readonly ColorFilter RedGrayscaleFilter =
        ColorFilter.CreateColorMatrix(
            ColorMatrix.UseRed + ColorMatrix.MapRedToGreenBlue + ColorMatrix.OpaqueAlphaOffset);

    public static readonly ColorFilter GreenGrayscaleFilter =
        ColorFilter.CreateColorMatrix(ColorMatrix.UseGreen + ColorMatrix.MapGreenToRedBlue +
                                      ColorMatrix.OpaqueAlphaOffset);

    public static readonly ColorFilter BlueGrayscaleFilter =
        ColorFilter.CreateColorMatrix(ColorMatrix.UseBlue + ColorMatrix.MapBlueToRedGreen +
                                      ColorMatrix.OpaqueAlphaOffset);

    public static readonly ColorFilter AlphaGrayscaleFilter =
        ColorFilter.CreateColorMatrix(ColorMatrix.MapAlphaToRedGreenBlue + ColorMatrix.OpaqueAlphaOffset);
}
