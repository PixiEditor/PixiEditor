using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace ChunkyImageLib;
public static class IReadOnlyChunkyImageEx
{
    /// <summary>
    ///     Draws the image onto the passed surface.
    /// </summary>
    /// <param name="image">Image to draw.</param>
    /// <param name="fullResRegion">A region inside an chunky image.</param>
    /// <param name="resolution">Chunk resolution.</param>
    /// <param name="surface">Surface to draw onto.</param>
    /// <param name="pos">Starting position on the surface.</param>
    /// <param name="paint">Paint that is used to draw.</param>
    public static void DrawMostUpToDateRegionOn
        (this IReadOnlyChunkyImage image, RectI fullResRegion, ChunkResolution resolution, DrawingSurface surface, VecI pos, Paint? paint = null)
    {
        surface.Canvas.Save();
        surface.Canvas.ClipRect(RectD.Create(pos, fullResRegion.Size));

        VecI chunkTopLeft = OperationHelper.GetChunkPos(fullResRegion.TopLeft, ChunkyImage.FullChunkSize);
        VecI chunkBotRight = OperationHelper.GetChunkPos(fullResRegion.BottomRight, ChunkyImage.FullChunkSize);
        VecI offsetFullRes = (chunkTopLeft * ChunkyImage.FullChunkSize) - fullResRegion.Pos;
        VecI offsetTargetRes = (VecI)(offsetFullRes * resolution.Multiplier());

        for (int j = chunkTopLeft.Y; j <= chunkBotRight.Y; j++)
        {
            for (int i = chunkTopLeft.X; i <= chunkBotRight.X; i++)
            {
                var chunkPos = new VecI(i, j);
                image.DrawMostUpToDateChunkOn(chunkPos, resolution, surface, offsetTargetRes + (chunkPos - chunkTopLeft) * resolution.PixelSize() + pos, paint);
            }
        }

        surface.Canvas.Restore();
    }
}
