using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using SkiaSharp;

namespace ChunkyImageLib;
public static class IReadOnlyChunkyImageEx
{
    public static void DrawMostUpToDateRegionOn
        (this IReadOnlyChunkyImage image, RectI fullResRegion, ChunkResolution resolution, SKSurface surface, VecI pos, SKPaint? paint = null)
    {
        VecI chunkTopLeft = OperationHelper.GetChunkPos(fullResRegion.TopLeft, ChunkyImage.FullChunkSize);
        VecI chunkBotRigth = OperationHelper.GetChunkPos(fullResRegion.BottomRight, ChunkyImage.FullChunkSize);
        VecI offsetFullRes = (chunkTopLeft * ChunkyImage.FullChunkSize) - fullResRegion.Pos;
        VecI offsetTargetRes = (VecI)(offsetFullRes * resolution.Multiplier());

        for (int j = chunkTopLeft.Y; j <= chunkBotRigth.Y; j++)
        {
            for (int i = chunkTopLeft.X; i <= chunkBotRigth.X; i++)
            {
                var chunkPos = new VecI(i, j);
                image.DrawMostUpToDateChunkOn(chunkPos, resolution, surface, offsetTargetRes + chunkPos * resolution.PixelSize() + pos, paint);
            }
        }
    }
}
