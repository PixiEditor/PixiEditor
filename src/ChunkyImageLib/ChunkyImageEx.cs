using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.PaintImpl;

namespace ChunkyImageLib;
public static class IReadOnlyChunkyImageEx
{
    public static void DrawMostUpToDateRegionOn
        (this IReadOnlyChunkyImage image, RectI fullResRegion, ChunkResolution resolution, DrawingSurface surface, VecI pos, Paint? paint = null)
    {
        surface.Canvas.Save();
        surface.Canvas.ClipRect(RectD.Create(pos, fullResRegion.Size));

        VecI chunkTopLeft = OperationHelper.GetChunkPos(fullResRegion.TopLeft, ChunkyImage.FullChunkSize);
        VecI chunkBotRigth = OperationHelper.GetChunkPos(fullResRegion.BottomRight, ChunkyImage.FullChunkSize);
        VecI offsetFullRes = (chunkTopLeft * ChunkyImage.FullChunkSize) - fullResRegion.Pos;
        VecI offsetTargetRes = (VecI)(offsetFullRes * resolution.Multiplier());

        for (int j = chunkTopLeft.Y; j <= chunkBotRigth.Y; j++)
        {
            for (int i = chunkTopLeft.X; i <= chunkBotRigth.X; i++)
            {
                var chunkPos = new VecI(i, j);
                image.DrawMostUpToDateChunkOn(chunkPos, resolution, surface, offsetTargetRes + (chunkPos - chunkTopLeft) * resolution.PixelSize() + pos, paint);
            }
        }

        surface.Canvas.Restore();
    }
}
