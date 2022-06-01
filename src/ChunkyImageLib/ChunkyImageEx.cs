using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using SkiaSharp;

namespace ChunkyImageLib;
public static class IReadOnlyChunkyImageEx
{
    public static void DrawMostUpToDateRegionOn
        (this IReadOnlyChunkyImage image, SKRectI fullResRegion, ChunkResolution resolution, SKSurface surface, VecI pos, SKPaint? paint = null)
    {
        VecI chunkTopLeft = OperationHelper.GetChunkPos(fullResRegion.Location, ChunkyImage.FullChunkSize);
        VecI chunkBotRigth = OperationHelper.GetChunkPos(fullResRegion.Location + fullResRegion.Size, ChunkyImage.FullChunkSize);
        VecI offsetFullRes = (chunkTopLeft * ChunkyImage.FullChunkSize) - (VecI)fullResRegion.Location;
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
